# NSL Library
Network Socket Library for development application with need network transfer data (support unity & blazor-client)

Platform - .NET 8/.NET Standard 2.0+

It's library always in develop process, I'm use this for my projects and you can free use this library on MIT license for yoursalfe

# NSL Unity
All unity supported projects has separate to solution NSL.Unity.sln
For success build you must
- extract "UnityEngine.dll" file from "/Reference/UnityEngine.zip" to "/Reference/UnityEngine.dll"
- set "UnityDebug" or "Unity" configuration

All unity supported multithread function/handles have extension methods ends with words "ForUnity"/"Unity" (as example - AddConnectHandleForUnity), if this methods does not exists - base function normal work in this platform

For correct working - you must have one or more ThreadHelper instance on you game scene

Unity for WebGL not support Request-Response logic with async/await method as default - we use Response delegate handle logic on self projects for normal working

# Documentation
in process

# Samples
In main project(NSL.sln) - change configuration to DebugExamples for
- ASP.NET
- Application
- for Unity have small project(/Examples/UnityExample/...)
And select interested project for "run as default"

# In library have implemented 
- TCP network protocol
- UDP network protocol
- WebSockets network protocol(extensions and wrappers for inject to ASP.NET)
- Local network wrapper
- Request-Response client alghoritms
- (Roslyn) RPC Generator
- (Roslyn) Linq/Queryable Select Generator
- (Roslyn) Fill Type Generator
- (Roslyn) Binary IO Generator
- Configuration Extensions(extensions and wrappers for inject to ASP.NET)
- Logger Extensions(extensions and wrappers for inject to ASP.NET)
- Session Extensions
- Version Extensions
- .NET Scripts

# Another projects based on NSL
- https://github.com/Timka654/NSL.Node - implementation basic room logic with load balancing(in process)
