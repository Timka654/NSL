using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphSharp.Controls;

namespace SelectGraphTool
{
    [Command(PackageIds.ShowSelectGraphTool)]
    internal sealed class ModelGraphWindowCommand : BaseCommand<ModelGraphWindowCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return ModelGraphWindow.ShowAsync();
        }
    }
    public class ModelGraphWindow : BaseToolWindow<ModelGraphWindow>
    {
        public override string GetTitle(int toolWindowId) => "My Tool Window";

        public override Type PaneType => typeof(Pane);

        public override async Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            //Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

            var wndControl = new ModelGraphControl();

            wndControl.Reload();

            return wndControl;
        }


        [Guid("b9c3e72b-f3eb-438c-b716-8e665655ada6")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
    public class ModelNodeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return DefaultTemplate;
        }
    }

    /// <summary>
    /// Interaction logic for ModelGraphControl.xaml
    /// </summary>
    public partial class ModelGraphControl : UserControl
    {

        private readonly ModelNodeTemplateSelector _templateSelector;

        public ModelGraphControl()
        {
            InitializeComponent();


            var nodeTemplate = new DataTemplate(typeof(ModelNode));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.BorderBrushProperty, System.Windows.Media.Brushes.DarkSlateGray);
            border.SetValue(Border.BackgroundProperty, System.Windows.Media.Brushes.LightGray);
            border.SetValue(Border.PaddingProperty, new Thickness(4));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));

            var panel = new FrameworkElementFactory(typeof(StackPanel));

            var nameText = new FrameworkElementFactory(typeof(TextBlock));
            nameText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Name"));
            nameText.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            panel.AppendChild(nameText);

            var itemsControl = new FrameworkElementFactory(typeof(ItemsControl));
            itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding("Properties"));

            var itemTemplate = new DataTemplate();
            var itemText = new FrameworkElementFactory(typeof(TextBlock));
            itemText.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("."));
            itemText.SetValue(TextBlock.MarginProperty, new Thickness(10, 0, 0, 0));
            itemTemplate.VisualTree = itemText;

            itemsControl.SetValue(ItemsControl.ItemTemplateProperty, itemTemplate);
            panel.AppendChild(itemsControl);

            border.AppendChild(panel);
            nodeTemplate.VisualTree = border;

            layout = new ModelGraphLayout();
            layout.Loaded += (_, _) =>
            {
                var controlTemplate = new ControlTemplate(typeof(VertexControl));
                var cp = new FrameworkElementFactory(typeof(ContentPresenter));
                cp.SetValue(ContentPresenter.ContentTemplateProperty, nodeTemplate);
                cp.SetBinding(ContentPresenter.ContentProperty, new System.Windows.Data.Binding("Vertex"));
                controlTemplate.VisualTree = cp;

                foreach (var vc in layout.Children.OfType<VertexControl>())
                {
                    vc.Template = controlTemplate;
                    vc.ApplyTemplate();
                }
            };

        }

        public async void Reload()
        {


            var dte = (EnvDTE.DTE)ToolkitPackage.GetGlobalService(typeof(SDTE));
            var solutionPath = dte?.Solution?.FullName;

            var graphVM = new GraphViewModel();
            if (!string.IsNullOrEmpty(solutionPath))
            {
                var workspace = MSBuildWorkspace.Create();
                var solution = await workspace.OpenSolutionAsync(solutionPath);


                foreach (var project in solution.Projects)
                {
                    var compilation = await project.GetCompilationAsync();
                    foreach (var syntaxTree in compilation.SyntaxTrees)
                    {
                        var semanticModel = compilation.GetSemanticModel(syntaxTree);
                        var root = await syntaxTree.GetRootAsync();

                        var classes = root.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>();
                        foreach (var cls in classes)
                        {
                            var symbol = semanticModel.GetDeclaredSymbol(cls);
                            foreach (var attr in symbol.GetAttributes())
                            {
                                var attrName = attr.AttributeClass.Name;
                                var sourceModel = symbol.Name;

                                if (attrName == "SelectGenerateAttribute" || attrName == "SelectGenerateIncludeAttribute")
                                {
                                    foreach (var arg in attr.ConstructorArguments)
                                    {
                                        if (arg.Kind == TypedConstantKind.Array)
                                            foreach (var val in arg.Values)
                                                graphVM.AddEdge(sourceModel, val.Value?.ToString(), attrName);
                                        else
                                            graphVM.AddEdge(sourceModel, arg.Value?.ToString(), attrName);
                                    }
                                }
                                else if (attrName == "SelectGenerateJoinAttribute")
                                {
                                    if (attr.ConstructorArguments.Length >= 2)
                                    {
                                        var baseModel = attr.ConstructorArguments[0].Value?.ToString();
                                        var joins = attr.ConstructorArguments[1].Values;
                                        foreach (var join in joins)
                                            graphVM.AddEdge(baseModel, join.Value?.ToString(), attrName);
                                    }
                                }
                                else if (attrName == "SelectGenerateProxyAttribute")
                                {
                                    if (attr.ConstructorArguments.Length == 1)
                                    {
                                        var target = attr.ConstructorArguments[0].Value?.ToString();
                                        graphVM.AddEdge(sourceModel, target, attrName);
                                    }
                                    else if (attr.ConstructorArguments.Length == 2)
                                    {
                                        var from = attr.ConstructorArguments[0].Value?.ToString();
                                        var to = attr.ConstructorArguments[1].Value?.ToString();
                                        graphVM.AddEdge(from, to, attrName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.InitializeGraph(graphVM);
        }

        ModelGraphLayout layout;

        public void InitializeGraph(GraphViewModel viewModel)
        {
            layout.Graph = viewModel.Graph;

            // Устанавливаем после Graph
            if (viewModel.Graph.VertexCount > 0)
            {
                layout.LayoutAlgorithmType = "EfficientSugiyama";
                layout.OverlapRemovalAlgorithmType = "FSA";
            }

            layout.HighlightAlgorithmType = "Simple";

            GraphHost.Content = layout;
        }

        private void ReloadItems_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }
    }
}
