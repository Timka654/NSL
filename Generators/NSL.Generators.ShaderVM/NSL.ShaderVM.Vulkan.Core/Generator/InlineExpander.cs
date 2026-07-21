using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.ShaderVM.Vulkan
{
    /// <summary>
    /// Phase 2: Expands inline method calls in a C# AST.
    /// Operates on blocks, replacing [ShaderFunction(Kind=Inline)] calls with their body.
    /// Uses a result-tuple pattern (preamble + expression) instead of shared mutable state.
    /// </summary>
    public partial class VulkanSourceGenerator
    {
    private class InlineExpander
    {
        private readonly ShaderBuildPlan _plan;
        private readonly int _inlineIdBase;
        private int _inlineCounter;

        public InlineExpander(ShaderBuildPlan plan, int inlineIdBase = 0)
        {
            _plan = plan;
            _inlineIdBase = inlineIdBase;
        }

        /// <summary>Build a map of C# method name → GLSL name for [ShaderFunction] Core methods in the containing type.
        /// Used by Substitutor to resolve calls like Exp → exp, Sqrt → sqrt inside inline bodies.
        /// EXCLUDES Inline methods — those must be expanded by the InlineExpander, not emitted as GLSL calls.</summary>
        private static Dictionary<string, string> BuildGlslFuncMap(IMethodSymbol methodSym)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (methodSym.ContainingType == null) return result;

            foreach (var member in methodSym.ContainingType.GetMembers())
            {
                if (!(member is IMethodSymbol m)) continue;
                foreach (var attr in m.GetAttributes())
                {
                    var an = attr.AttributeClass?.Name;
                    if (an != "ShaderFunctionAttribute" && an != "ShaderFunction") continue;

                    // Skip Inline methods — they are expanded by InlineExpander, not emitted as GLSL calls
                    var kind = ShaderFunctionKind.Core;
                    foreach (var na in attr.NamedArguments)
                    {
                        if (na.Key == "Kind" && na.Value.Value is int kv) kind = (ShaderFunctionKind)kv;
                        if (na.Key == "Inline" && na.Value.Value is bool ib && ib) kind = ShaderFunctionKind.Inline;
                        if (na.Key == "CSharpCode" && na.Value.Value is bool cb && cb) kind = ShaderFunctionKind.Managed;
                    }
                    if (kind == ShaderFunctionKind.Inline) break;

                    string tmpl = attr.ConstructorArguments.Length > 0
                        ? attr.ConstructorArguments[0].Value?.ToString() : null;
                    if (!string.IsNullOrEmpty(tmpl))
                    {
                        int p = tmpl.IndexOf('(');
                        string glslName = p >= 0 ? tmpl.Substring(0, p) : tmpl;
                        if (!result.ContainsKey(m.Name))
                            result[m.Name] = glslName;
                    }
                    break;
                }
            }
            return result;
        }

        /// <summary>Build a map of C# method name → IMethodSymbol for Inline methods in the containing type.</summary>
        private static Dictionary<string, IMethodSymbol> BuildInlineMethodMap(IMethodSymbol methodSym, ShaderBuildPlan plan)
        {
            var result = new Dictionary<string, IMethodSymbol>(StringComparer.Ordinal);
            if (methodSym.ContainingType == null) return result;

            foreach (var member in methodSym.ContainingType.GetMembers())
            {
                if (!(member is IMethodSymbol m)) continue;
                string qKey = (m.ContainingType?.Name ?? "") + "." + m.Name;
                if (plan.InlineMethods.ContainsKey(qKey) || plan.InlineMethods.ContainsKey(m.Name))
                {
                    if (!result.ContainsKey(m.Name))
                        result[m.Name] = m;
                }
            }
            return result;
        }

        /// <summary>
        /// Expand all inline calls in a block. Returns a new block with inlined code.
        /// If no inline expansion occurred, returns the ORIGINAL block to preserve syntax tree connectivity
        /// (so that GlslAstRewriter's semantic model lookups still work).
        /// This is the main entry point for Phase 2.
        /// </summary>
        public BlockSyntax ExpandBlock(BlockSyntax block)
        {
            if (block == null) return null;
            bool anyExpanded = false;
            var expandedStatements = new List<StatementSyntax>();

            foreach (var stmt in block.Statements)
            {
                if (stmt == null) continue;
                var (preambles, rewritten) = ExpandStatement(stmt);
                bool hasPreambles = preambles != null && preambles.Count > 0;
                bool stmtChanged = rewritten != null && !ReferenceEquals(rewritten, stmt);

                if (hasPreambles || stmtChanged)
                {
                    anyExpanded = true;
                    if (hasPreambles)
                    {
                        foreach (var p in preambles)
                            if (p != null) expandedStatements.Add(p);
                    }
                    if (rewritten != null)
                        expandedStatements.Add(rewritten);
                    else if (!hasPreambles)
                        expandedStatements.Add(stmt); // keep original if nothing to replace
                }
                else
                {
                    expandedStatements.Add(stmt);
                }
            }

            // If nothing was expanded, return the ORIGINAL block to preserve syntax tree links
            if (!anyExpanded)
                return block;

            return SyntaxFactory.Block(expandedStatements);
        }

        /// <summary>
        /// Expand inline calls in a single statement.
        /// Returns (preambleStatements, rewrittenStatement).
        /// Preamble statements must be emitted BEFORE the rewritten statement.
        /// </summary>
        private (List<StatementSyntax> Preambles, StatementSyntax Statement) ExpandStatement(StatementSyntax stmt)
        {
            if (stmt is BlockSyntax block)
                return (new List<StatementSyntax>(), ExpandBlock(block));

            if (stmt is ExpressionStatementSyntax exprStmt)
            {
                var (preambles, expr) = ExpandExpression(exprStmt.Expression);
                if (expr == null) return (preambles, null); // void inline removed
                // If expression didn't change (no inline expansion), return original statement
                if (ReferenceEquals(expr, exprStmt.Expression))
                    return (preambles ?? new List<StatementSyntax>(), stmt);
                return (preambles, exprStmt.WithExpression(expr));
            }

            if (stmt is LocalDeclarationStatementSyntax localDecl)
            {
                var expandedDecls = new List<VariableDeclaratorSyntax>();
                var allPreambles = new List<StatementSyntax>();
                bool changed = false;

                foreach (var variable in localDecl.Declaration.Variables)
                {
                    if (variable.Initializer != null)
                    {
                        var (preambles, newInit) = ExpandExpression(variable.Initializer.Value);
                        if (preambles != null) allPreambles.AddRange(preambles);
                        if (newInit != null && !ReferenceEquals(newInit, variable.Initializer.Value))
                        {
                            expandedDecls.Add(variable.WithInitializer(variable.Initializer.WithValue(newInit)));
                            changed = true;
                        }
                        else
                        {
                            expandedDecls.Add(variable);
                        }
                    }
                    else
                    {
                        expandedDecls.Add(variable);
                    }
                }

                if (expandedDecls.All(v => v.Initializer == null))
                    return (allPreambles, null);

                if (!changed && allPreambles.Count == 0)
                    return (new List<StatementSyntax>(), stmt); // nothing changed — return original

                var newDecl = localDecl.Declaration.WithVariables(SyntaxFactory.SeparatedList(expandedDecls));
                return (allPreambles, localDecl.WithDeclaration(newDecl));
            }

            if (stmt is ReturnStatementSyntax retStmt)
            {
                if (retStmt.Expression == null) return (new List<StatementSyntax>(), stmt);
                var (preambles, expr) = ExpandExpression(retStmt.Expression);
                if (ReferenceEquals(expr, retStmt.Expression))
                    return (preambles ?? new List<StatementSyntax>(), stmt);
                return (preambles, retStmt.WithExpression(expr));
            }

            if (stmt is IfStatementSyntax ifStmt)
                return ExpandIfStatement(ifStmt);

            if (stmt is ForStatementSyntax forStmt)
                return ExpandForStatement(forStmt);

            if (stmt is WhileStatementSyntax whileStmt)
                return ExpandWhileStatement(whileStmt);

            if (stmt is DoStatementSyntax doStmt)
                return ExpandDoStatement(doStmt);

            if (stmt is ReturnStatementSyntax retStmt2)
            {
                if (retStmt2.Expression == null) return (new List<StatementSyntax>(), stmt);
                var (preambles, expr) = ExpandExpression(retStmt2.Expression);
                return (preambles, retStmt2.WithExpression(expr));
            }

            // Default: visit children
            var visitor = new ExpressionExpander(this);
            var rewritten = (StatementSyntax)visitor.Visit(stmt);
            return (new List<StatementSyntax>(), rewritten);
        }

        private (List<StatementSyntax>, StatementSyntax) ExpandIfStatement(IfStatementSyntax ifStmt)
        {
            var allPreambles = new List<StatementSyntax>();

            // Condition — may contain inline calls
            var (condPreambles, condition) = ExpandExpression(ifStmt.Condition);
            allPreambles.AddRange(condPreambles);
            if (condition == null) return (allPreambles, null);

            // If body
            var (ifPreambles, ifBody) = ExpandStatement(ifStmt.Statement);
            if (ifPreambles.Count > 0)
                ifBody = PrependToBlock(ifBody, ifPreambles);
            // If body is null (void inline removed everything) — wrap with empty block
            if (ifBody == null)
                ifBody = SyntaxFactory.Block();

            // Else body
            ElseClauseSyntax elseClause = null;
            if (ifStmt.Else != null)
            {
                var (elsePreambles, elseBody) = ExpandStatement(ifStmt.Else.Statement);
                if (elsePreambles.Count > 0)
                    elseBody = PrependToBlock(elseBody, elsePreambles);
                if (elseBody != null)
                    elseClause = SyntaxFactory.ElseClause(elseBody);
            }

            return (allPreambles, SyntaxFactory.IfStatement(condition, ifBody, elseClause));
        }

        private (List<StatementSyntax>, StatementSyntax) ExpandForStatement(ForStatementSyntax forStmt)
        {
            var allPreambles = new List<StatementSyntax>();

            // Condition
            var (condPreambles, condition) = forStmt.Condition != null
                ? ExpandExpression(forStmt.Condition)
                : (new List<StatementSyntax>(), (ExpressionSyntax)null);
            if (condPreambles != null) allPreambles.AddRange(condPreambles);

            // Body
            var (bodyPreambles, body) = ExpandStatement(forStmt.Statement);
            if (bodyPreambles != null && bodyPreambles.Count > 0)
                body = PrependToBlock(body ?? forStmt.Statement, bodyPreambles);
            if (body == null) body = SyntaxFactory.Block();

            // If nothing changed, return original to preserve syntax tree links
            bool bodyUnchanged = ReferenceEquals(body, forStmt.Statement);
            bool condUnchanged = condition == null || ReferenceEquals(condition, forStmt.Condition);
            if (bodyUnchanged && condUnchanged && allPreambles.Count == 0)
                return (allPreambles, forStmt);

            var newFor = forStmt;
            if (condition != null && !ReferenceEquals(condition, forStmt.Condition))
                newFor = newFor.WithCondition(condition);
            if (!ReferenceEquals(body, forStmt.Statement))
                newFor = newFor.WithStatement(body);

            return (allPreambles, newFor);
        }

        private (List<StatementSyntax>, StatementSyntax) ExpandWhileStatement(WhileStatementSyntax whileStmt)
        {
            var allPreambles = new List<StatementSyntax>();

            var (condPreambles, condition) = ExpandExpression(whileStmt.Condition);
            if (condPreambles != null) allPreambles.AddRange(condPreambles);

            var (bodyPreambles, body) = ExpandStatement(whileStmt.Statement);
            if (bodyPreambles != null && bodyPreambles.Count > 0)
                body = PrependToBlock(body ?? whileStmt.Statement, bodyPreambles);
            if (body == null) body = SyntaxFactory.Block();

            bool condUnchanged = ReferenceEquals(condition, whileStmt.Condition);
            bool bodyUnchanged = ReferenceEquals(body, whileStmt.Statement);
            if (condUnchanged && bodyUnchanged && allPreambles.Count == 0)
                return (allPreambles, whileStmt);

            return (allPreambles, SyntaxFactory.WhileStatement(condition ?? whileStmt.Condition, body));
        }

        private (List<StatementSyntax>, StatementSyntax) ExpandDoStatement(DoStatementSyntax doStmt)
        {
            var (bodyPreambles, body) = ExpandStatement(doStmt.Statement);
            if (bodyPreambles != null && bodyPreambles.Count > 0)
                body = PrependToBlock(body ?? doStmt.Statement, bodyPreambles);
            if (body == null) body = SyntaxFactory.Block();

            var (condPreambles, condition) = ExpandExpression(doStmt.Condition);
            if (condPreambles != null && condPreambles.Count > 0)
                body = PrependToBlock(body, condPreambles);

            bool bodyUnchanged = ReferenceEquals(body, doStmt.Statement);
            bool condUnchanged = ReferenceEquals(condition, doStmt.Condition);
            if (bodyUnchanged && condUnchanged)
                return (new List<StatementSyntax>(), doStmt);

            return (new List<StatementSyntax>(),
                SyntaxFactory.DoStatement(body, condition ?? doStmt.Condition));
        }

        private StatementSyntax PrependToBlock(StatementSyntax body, List<StatementSyntax> preambles)
        {
            if (preambles.Count == 0) return body;
            if (body is BlockSyntax block)
            {
                var newStmts = new List<StatementSyntax>(preambles);
                newStmts.AddRange(block.Statements);
                return SyntaxFactory.Block(newStmts);
            }
            var stmts = new List<StatementSyntax>(preambles) { body };
            return SyntaxFactory.Block(stmts);
        }

        /// <summary>
        /// Expand inline calls in an expression.
        /// Returns (preambleStatements, rewrittenExpression).
        /// For inline calls: preamble contains declarations + do/while, expression is the result variable.
        /// For void inline calls: preamble contains the expanded block, expression is null.
        /// </summary>
        private (List<StatementSyntax> Preambles, ExpressionSyntax Expression) ExpandExpression(ExpressionSyntax expr)
        {
            if (expr == null) return (new List<StatementSyntax>(), null);

            if (expr is InvocationExpressionSyntax inv)
                return ExpandInvocation(inv);

            if (expr is AssignmentExpressionSyntax assign)
            {
                var (leftP, left) = ExpandExpression(assign.Left);
                var (rightP, right) = ExpandExpression(assign.Right);
                var p = new List<StatementSyntax>(leftP);
                p.AddRange(rightP);
                if (left == null || right == null) return (p, null);
                // If nothing changed, return original to preserve syntax tree links
                if (ReferenceEquals(left, assign.Left) && ReferenceEquals(right, assign.Right) && p.Count == 0)
                    return (p, assign);
                return (p, assign.WithLeft(left).WithRight(right));
            }

            if (expr is BinaryExpressionSyntax bin)
            {
                var (leftP, left) = ExpandExpression(bin.Left);
                var (rightP, right) = ExpandExpression(bin.Right);
                var p = new List<StatementSyntax>(leftP);
                p.AddRange(rightP);
                if (left == null || right == null) return (p, null);
                if (ReferenceEquals(left, bin.Left) && ReferenceEquals(right, bin.Right) && p.Count == 0)
                    return (p, bin);
                return (p, bin.WithLeft(left).WithRight(right));
            }

            if (expr is ConditionalExpressionSyntax cond)
            {
                var (condP, condExpr) = ExpandExpression(cond.Condition);
                var (trueP, trueExpr) = ExpandExpression(cond.WhenTrue);
                var (falseP, falseExpr) = ExpandExpression(cond.WhenFalse);
                var p = new List<StatementSyntax>(condP);
                p.AddRange(trueP);
                p.AddRange(falseP);
                if (condExpr == null) return (p, null);
                if (ReferenceEquals(condExpr, cond.Condition) && ReferenceEquals(trueExpr, cond.WhenTrue)
                    && ReferenceEquals(falseExpr, cond.WhenFalse) && p.Count == 0)
                    return (p, cond);
                return (p, cond.WithCondition(condExpr).WithWhenTrue(trueExpr).WithWhenFalse(falseExpr));
            }

            if (expr is PrefixUnaryExpressionSyntax prefix)
            {
                var (opP, op) = ExpandExpression(prefix.Operand);
                if (op == null) return (opP, null);
                if (ReferenceEquals(op, prefix.Operand) && opP.Count == 0)
                    return (opP, prefix);
                return (opP, prefix.WithOperand(op));
            }

            if (expr is PostfixUnaryExpressionSyntax postfix)
            {
                var (opP, op) = ExpandExpression(postfix.Operand);
                if (op == null) return (opP, null);
                if (ReferenceEquals(op, postfix.Operand) && opP.Count == 0)
                    return (opP, postfix);
                return (opP, postfix.WithOperand(op));
            }

            if (expr is ParenthesizedExpressionSyntax paren)
            {
                var (innerP, inner) = ExpandExpression(paren.Expression);
                if (inner == null) return (innerP, null);
                if (ReferenceEquals(inner, paren.Expression) && innerP.Count == 0)
                    return (innerP, paren);
                return (innerP, paren.WithExpression(inner));
            }

            // Default: no expansion needed — return original expression
            return (new List<StatementSyntax>(), expr);
        }

        /// <summary>
        /// Expand an invocation. If it's an inline call, produce the expanded code.
        /// Otherwise, expand arguments recursively.
        /// </summary>
        private (List<StatementSyntax> Preambles, ExpressionSyntax Expression) ExpandInvocation(InvocationExpressionSyntax inv)
        {
            // First, expand arguments
            var argPreambles = new List<StatementSyntax>();
            var expandedArgs = new List<ArgumentSyntax>();
            bool argsChanged = false;

            foreach (var arg in inv.ArgumentList.Arguments)
            {
                var (p, expr) = ExpandExpression(arg.Expression);
                if (p != null && p.Count > 0) argPreambles.AddRange(p);
                if (expr != null && !ReferenceEquals(expr, arg.Expression))
                {
                    expandedArgs.Add(arg.WithExpression(expr));
                    argsChanged = true;
                }
                else
                {
                    expandedArgs.Add(arg);
                }
            }

            // Check if this is an inline call
            IMethodSymbol inlineSym = ResolveInlineSymbol(inv);
            if (inlineSym == null)
            {
                // Not inline — return original if args unchanged
                if (!argsChanged && argPreambles.Count == 0)
                    return (argPreambles, inv);
                var newArgList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(expandedArgs));
                return (argPreambles, inv.WithArgumentList(newArgList));
            }

            // It's an inline call — expand the body
            var inlineArgList = argsChanged
                ? SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(expandedArgs))
                : inv.ArgumentList;
            var inlineInv = argsChanged ? inv.WithArgumentList(inlineArgList) : inv;

            var entry = _plan.FindInlineBySymbol(inlineSym);
            if (entry?.Syntax == null)
                return (argPreambles, inlineInv); // Can't find syntax — skip

            return ExpandInlineBody(inlineSym, entry.Syntax, inlineInv, argPreambles);
        }

        /// <summary>
        /// Check if an invocation resolves to a known inline method.
        /// </summary>
        private IMethodSymbol ResolveInlineSymbol(InvocationExpressionSyntax inv)
        {
            string name = null;
            if (inv.Expression is IdentifierNameSyntax id)
                name = id.Identifier.Text;
            else if (inv.Expression is MemberAccessExpressionSyntax ma)
                name = ma.Name.Identifier.Text;

            if (name == null) return null;

            // Try simple name first (same-class inlines)
            if (_plan.InlineMethods.TryGetValue(name, out var entry))
                return entry.Symbol;

            // Try all inline entries — the simple name may have been registered under a qualified key
            foreach (var kv in _plan.InlineMethods)
            {
                if (kv.Key.EndsWith("." + name) || kv.Key == name)
                    return kv.Value.Symbol;
            }

            return null;
        }

        /// <summary>
        /// Core inline expansion logic. Replaces an inline call with its body,
        /// using proper block structure for all cases.
        /// </summary>
        private (List<StatementSyntax> Preambles, ExpressionSyntax Expression) ExpandInlineBody(
            IMethodSymbol methodSym,
            MethodDeclarationSyntax methodSyntax,
            InvocationExpressionSyntax rewrittenCall,
            List<StatementSyntax> argPreambles)
        {
            int inlineId = ++_inlineCounter + _inlineIdBase;
            string prefix = $"_inl{inlineId}_";

            // Build param → argument map
            var argMap = new Dictionary<string, ExpressionSyntax>();
            var outDeclarations = new List<LocalDeclarationStatementSyntax>();
            var paramList = methodSym.Parameters;
            var args = rewrittenCall.ArgumentList.Arguments;

            for (int i = 0; i < paramList.Length && i < args.Count; i++)
            {
                var expr = args[i].Expression;
                if (args[i].RefOrOutKeyword.Kind() != SyntaxKind.None
                    && expr is DeclarationExpressionSyntax declExpr
                    && declExpr.Designation is SingleVariableDesignationSyntax svd)
                {
                    outDeclarations.Add(SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(declExpr.Type,
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(svd.Identifier)))));
                    expr = SyntaxFactory.IdentifierName(svd.Identifier);
                }
                argMap[paramList[i].Name] = expr;
            }

            // Collect locals to rename
            var localNames = new HashSet<string>();
            if (methodSyntax.Body != null)
            {
                foreach (var local in methodSyntax.Body.DescendantNodes().OfType<VariableDeclaratorSyntax>())
                    localNames.Add(local.Identifier.Text);
            }

            // Build [ShaderFunction] resolution maps from the containing type
            // This allows inline bodies to call Core/Managed methods (Exp → exp, Sqrt → sqrt)
            var glslFuncMap = BuildGlslFuncMap(methodSym);
            var inlineMethodMap = BuildInlineMethodMap(methodSym, _plan);

            // Expression body — simple substitution
            if (methodSyntax.ExpressionBody != null)
            {
                var substitutor = new Substitutor(argMap, localNames, prefix, _plan, glslFuncMap, inlineMethodMap);
                var expanded = substitutor.Visit(methodSyntax.ExpressionBody.Expression);
                if (expanded == null) return (argPreambles, rewrittenCall);

                var allPreambles = new List<StatementSyntax>(argPreambles);
                allPreambles.AddRange(outDeclarations);
                return (allPreambles, (ExpressionSyntax)expanded);
            }

            if (methodSyntax.Body == null)
                return (argPreambles, rewrittenCall);

            var retStmts = methodSyntax.Body.DescendantNodes().OfType<ReturnStatementSyntax>().ToList();

            // ── Void method ──
            if (methodSym.ReturnsVoid)
            {
                var expandedBody = ExpandBlockWithSubstitutor(methodSyntax.Body, argMap, localNames, prefix, glslFuncMap, inlineMethodMap);
                var allPreambles = new List<StatementSyntax>(argPreambles);
                allPreambles.AddRange(outDeclarations);

                if (retStmts.Count == 0)
                {
                    // No returns — just emit the expanded body statements into preambles
                    foreach (var s in expandedBody.Statements)
                        allPreambles.Add(s);
                    return (allPreambles, (ExpressionSyntax)null); // void — no expression
                }

                // Has returns → do { ... } while(false) with break conversion
                bool hasLoops = methodSyntax.Body.DescendantNodes().Any(n =>
                    n is WhileStatementSyntax || n is ForStatementSyntax || n is DoStatementSyntax);
                string loopFlag = hasLoops ? $"_inl{inlineId}_done" : null;

                var stmts = ExpandBlockWithReturnConversion(methodSyntax.Body, argMap, localNames, prefix, null, loopFlag, glslFuncMap, inlineMethodMap);
                var doBlock = SyntaxFactory.Block(stmts);

                if (loopFlag != null)
                    allPreambles.Add(MakeBoolDecl(loopFlag, false));
                allPreambles.Add(SyntaxFactory.DoStatement(doBlock, FalseLiteral));

                return (allPreambles, null); // void — no expression
            }

            // ── Non-void method ──
            if (retStmts.Count == 1 && methodSyntax.Body.Statements.Count == 1)
            {
                // Single return as only statement — simple substitution
                var substitutor = new Substitutor(argMap, localNames, prefix, _plan, glslFuncMap, inlineMethodMap);
                var expanded = substitutor.Visit(retStmts[0].Expression);
                if (expanded == null) return (argPreambles, rewrittenCall);

                var allPreambles = new List<StatementSyntax>(argPreambles);
                allPreambles.AddRange(outDeclarations);
                return (allPreambles, SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)expanded));
            }

            // Single return at end of multi-statement body (e.g., PcgNext: assignments + return at end)
            // Emit preceding statements as preambles, return expression as inline result — no do/while needed
            if (retStmts.Count == 1)
            {
                var lastStmt = methodSyntax.Body.Statements.Last();
                if (lastStmt is ReturnStatementSyntax retAtEnd && retAtEnd == retStmts[0]
                    && !methodSyntax.Body.DescendantNodes().Any(n =>
                        n is WhileStatementSyntax || n is ForStatementSyntax || n is DoStatementSyntax))
                {
                    // All preceding statements go into preamble, return expression becomes the result
                    var substitutor = new Substitutor(argMap, localNames, prefix, _plan, glslFuncMap, inlineMethodMap);
                    var allPreambles = new List<StatementSyntax>(argPreambles);
                    allPreambles.AddRange(outDeclarations);

                    foreach (var s in methodSyntax.Body.Statements)
                    {
                        if (s == lastStmt) break; // Don't include the return itself
                        var expanded = (StatementSyntax)substitutor.Visit(s);
                        if (expanded != null) allPreambles.Add(expanded);
                    }

                    var retExpr = substitutor.Visit(retAtEnd.Expression);
                    if (retExpr == null) return (argPreambles, rewrittenCall);
                    return (allPreambles, SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)retExpr));
                }
            }

            // Multi-return → do { ... } while(false) with temp variable
            {
                string tmpVar = $"_inl{inlineId}_r";
                string tmpType = GlslTypeMapper.MapType(methodSym.ReturnType.Name);

                bool hasLoops = methodSyntax.Body.DescendantNodes().Any(n =>
                    n is WhileStatementSyntax || n is ForStatementSyntax || n is DoStatementSyntax);
                string loopFlag = hasLoops ? $"_inl{inlineId}_done" : null;

                var stmts = ExpandBlockWithReturnConversion(methodSyntax.Body, argMap, localNames, prefix, tmpVar, loopFlag, glslFuncMap, inlineMethodMap);
                var doBlock = SyntaxFactory.Block(stmts);

                var allPreambles = new List<StatementSyntax>(argPreambles);
                allPreambles.AddRange(outDeclarations);
                if (loopFlag != null)
                    allPreambles.Add(MakeBoolDecl(loopFlag, false));
                allPreambles.Add(MakeVarDecl(tmpType, tmpVar));
                allPreambles.Add(SyntaxFactory.DoStatement(doBlock, FalseLiteral));

                return (allPreambles, SyntaxFactory.IdentifierName(tmpVar));
            }
        }

        /// <summary>Expand a block body using the Substitutor (parameter → argument, local renaming).</summary>
        private BlockSyntax ExpandBlockWithSubstitutor(
            BlockSyntax body,
            Dictionary<string, ExpressionSyntax> argMap,
            HashSet<string> localNames,
            string prefix,
            Dictionary<string, string> glslFuncMap = null,
            Dictionary<string, IMethodSymbol> inlineMethodMap = null)
        {
            var substitutor = new Substitutor(argMap, localNames, prefix, _plan, glslFuncMap, inlineMethodMap);
            var expandedStmts = new List<StatementSyntax>();

            foreach (var stmt in body.Statements)
            {
                if (stmt == null) continue;
                var (preambles, rewritten) = ExpandStatementWithSubstitutor(stmt, substitutor);
                if (preambles != null)
                    foreach (var p in preambles)
                        if (p != null) expandedStmts.Add(p);
                if (rewritten != null)
                    expandedStmts.Add(rewritten);
            }

            return SyntaxFactory.Block(expandedStmts);
        }

        /// <summary>Expand a block with return-to-break conversion and optional result variable.</summary>
        private List<StatementSyntax> ExpandBlockWithReturnConversion(
            BlockSyntax body,
            Dictionary<string, ExpressionSyntax> argMap,
            HashSet<string> localNames,
            string prefix,
            string resultVar,
            string loopFlag,
            Dictionary<string, string> glslFuncMap = null,
            Dictionary<string, IMethodSymbol> inlineMethodMap = null)
        {
            var substitutor = new SubstitutorWithReturns(argMap, localNames, prefix, _plan, resultVar, loopFlag, glslFuncMap, inlineMethodMap);
            var expandedStmts = new List<StatementSyntax>();

            foreach (var stmt in body.Statements)
            {
                if (stmt == null) continue;
                var (preambles, rewritten) = ExpandStatementWithSubstitutor(stmt, substitutor);
                if (preambles != null)
                    foreach (var p in preambles)
                        if (p != null) expandedStmts.Add(p);
                if (rewritten != null)
                    expandedStmts.Add(rewritten);
            }

            return expandedStmts;
        }

        /// <summary>Expand a single statement through a Substitutor, handling inline expansion recursively.</summary>
        private (List<StatementSyntax>, StatementSyntax) ExpandStatementWithSubstitutor(
            StatementSyntax stmt,
            CSharpSyntaxRewriter substitutor)
        {
            // Apply parameter substitution first
            var substituted = (StatementSyntax)substitutor.Visit(stmt);
            if (substituted == null) return (new List<StatementSyntax>(), null);

            // Then expand inline calls in the substituted statement
            return ExpandStatement(substituted);
        }

        // ── Statement/Expression helpers ──

        private static readonly LiteralExpressionSyntax FalseLiteral =
            SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
        private static readonly LiteralExpressionSyntax TrueLiteral =
            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);

        private static LocalDeclarationStatementSyntax MakeBoolDecl(string name, bool value)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("bool"),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(name)
                            .WithInitializer(SyntaxFactory.EqualsValueClause(
                                value ? TrueLiteral : FalseLiteral)))));
        }

        private static LocalDeclarationStatementSyntax MakeVarDecl(string typeName, string name)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(typeName),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(name))));
        }

        // ══════════════════════════════════════════════════════════════
        // Substitutors: replace parameter names with arguments, rename locals
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Basic substitutor: replaces parameter names with argument expressions,
        /// renames local variables to avoid conflicts.
        /// Also resolves inline calls within the body recursively.
        /// </summary>
        private class Substitutor : CSharpSyntaxRewriter
        {
            protected readonly Dictionary<string, ExpressionSyntax> _argMap;
            protected readonly HashSet<string> _localNames;
            protected readonly string _prefix;
            protected readonly ShaderBuildPlan _plan;
            protected readonly Dictionary<string, string> _glslFuncMap;
            protected readonly Dictionary<string, IMethodSymbol> _inlineMethodMap;

            public Substitutor(Dictionary<string, ExpressionSyntax> argMap, HashSet<string> localNames, string prefix, ShaderBuildPlan plan,
                Dictionary<string, string> glslFuncMap = null, Dictionary<string, IMethodSymbol> inlineMethodMap = null)
            {
                _argMap = argMap;
                _localNames = localNames;
                _prefix = prefix;
                _plan = plan;
                _glslFuncMap = glslFuncMap;
                _inlineMethodMap = inlineMethodMap;
            }

            public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                string name = node.Identifier.Text;
                if (_localNames.Contains(name))
                    node = node.WithIdentifier(SyntaxFactory.Identifier(_prefix + name));
                return base.VisitVariableDeclarator(node);
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                string name = node.Identifier.Text;
                if (_argMap.TryGetValue(name, out var argExpr))
                {
                    if (argExpr is BinaryExpressionSyntax || argExpr is ConditionalExpressionSyntax)
                        return SyntaxFactory.ParenthesizedExpression(argExpr);
                    return argExpr;
                }
                if (_localNames.Contains(name))
                    return SyntaxFactory.IdentifierName(_prefix + name);
                return base.VisitIdentifierName(node);
            }

            // Convert C# casts to GLSL function-style casts: (float)x → float(x)
            public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
            {
                string glslType = GlslTypeMapper.MapType(node.Type.ToString());
                var expr = (ExpressionSyntax)Visit(node.Expression);
                if (expr == null) return null;
                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName(glslType),
                    SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expr))));
            }

            // Resolve [ShaderFunction]-tagged calls inside inline bodies (Exp → exp, Sqrt → sqrt, etc.)
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.Expression is IdentifierNameSyntax idName)
                {
                    string csName = idName.Identifier.Text;

                    // 1. Core/Managed functions from containing type: emit as GLSL function call
                    if (_glslFuncMap != null && _glslFuncMap.TryGetValue(csName, out var glslName))
                    {
                        var visitedArgs = (ArgumentListSyntax)Visit(node.ArgumentList);
                        return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.IdentifierName(glslName), visitedArgs);
                    }

                    // 2. Try plan's MethodNameMap (qualified key)
                    // Inline bodies may call methods from other classes
                    var sym = _plan?.MethodNameMap; // just check if plan exists

                    // 3. Nested inline: recursively expand
                    if (_inlineMethodMap != null && _inlineMethodMap.TryGetValue(csName, out var nestedSym))
                    {
                        var visitedArgs = (ArgumentListSyntax)Visit(node.ArgumentList);
                        // The InlineExpander will handle the expansion
                        var entry = _plan?.FindInlineBySymbol(nestedSym);
                        if (entry?.Syntax != null)
                        {
                            // Inline expansion — but we're inside a Substitutor, need to delegate back to InlineExpander
                            // For now, return the visited invocation (InlineExpander.ExpandBlock will catch it)
                            return SyntaxFactory.InvocationExpression(idName, visitedArgs);
                        }
                    }
                }
                return base.VisitInvocationExpression(node);
            }
        }

        /// <summary>
        /// Substitutor with return-to-break conversion for inline expansion.
        /// Converts `return expr;` → `_resultVar = expr; break;` (or just `break;` for void).
        /// </summary>
        private class SubstitutorWithReturns : Substitutor
        {
            private readonly string _resultVar;
            private readonly string _loopFlag;

            public SubstitutorWithReturns(
                Dictionary<string, ExpressionSyntax> argMap,
                HashSet<string> localNames,
                string prefix,
                ShaderBuildPlan plan,
                string resultVar,
                string loopFlag,
                Dictionary<string, string> glslFuncMap = null,
                Dictionary<string, IMethodSymbol> inlineMethodMap = null)
                : base(argMap, localNames, prefix, plan, glslFuncMap, inlineMethodMap)
            {
                _resultVar = resultVar;
                _loopFlag = loopFlag;
            }

            public override SyntaxNode VisitReturnStatement(ReturnStatementSyntax node)
            {
                var stmts = new List<StatementSyntax>();

                if (_loopFlag != null)
                    stmts.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(_loopFlag), TrueLiteral)));

                if (!string.IsNullOrEmpty(_resultVar) && node.Expression != null)
                {
                    var visitedExpr = (ExpressionSyntax)Visit(node.Expression);
                    stmts.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(_resultVar), visitedExpr)));
                }

                stmts.Add(SyntaxFactory.BreakStatement());
                return SyntaxFactory.Block(stmts.ToArray());
            }

            // Wrap loop bodies to check flag after each loop iteration
            private StatementSyntax WrapWithFlagCheck(StatementSyntax stmt)
            {
                if (_loopFlag == null) return stmt;
                var ifBreak = SyntaxFactory.IfStatement(
                    SyntaxFactory.IdentifierName(_loopFlag),
                    SyntaxFactory.BreakStatement());
                return SyntaxFactory.Block(stmt, ifBreak);
            }

            public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
            {
                return WrapWithFlagCheck((StatementSyntax)base.VisitWhileStatement(node));
            }

            public override SyntaxNode VisitForStatement(ForStatementSyntax node)
            {
                return WrapWithFlagCheck((StatementSyntax)base.VisitForStatement(node));
            }

            public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
            {
                return WrapWithFlagCheck((StatementSyntax)base.VisitDoStatement(node));
            }
        }

        /// <summary>
        /// Expression-level visitor for cases where we just need to traverse
        /// without the full ExpandExpression machinery.
        /// </summary>
        private class ExpressionExpander : CSharpSyntaxRewriter
        {
            private readonly InlineExpander _expander;

            public ExpressionExpander(InlineExpander expander) { _expander = expander; }

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                // Check if this is an inline call
                IMethodSymbol inlineSym = _expander.ResolveInlineSymbol(node);
                if (inlineSym != null)
                {
                    var entry = _expander._plan.FindInlineBySymbol(inlineSym);
                    if (entry?.Syntax != null)
                    {
                        // Expand inline — but we're in expression context, preamble goes to parent block
                        // This is handled by ExpandInvocation at the statement level
                        // At this level, we just mark it for expansion
                    }
                }
                return base.VisitInvocationExpression(node);
            }
        }
    }
    }
}
