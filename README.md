# excogitated.common
This is a C# library that I've built gradually over the last decade of software development. Most classes were created to simplify development and reduce code. I use this library in most of the projects I work on and if anyone else finds it useful, great.

Currently working on documentation and test cases for the contained classes. Feels like that might take longer than it took to actually write them. Until that is completed it'll remain in an alpha state.

# NuGet Package:
https://www.nuget.org/packages/excogitated.common/

# Notable classes:
Module - Micro IOC framework using static generic fields. Should be faster than any other solution available but has an extremely limited feature set.
  
AsyncQueue - Created before IAsyncEnumerable was available but still useful for multiple producer and or multiple consumer scenarios.

Extensions_AsyncEnumerable - Most of the Linq extensions available for IEnumerable rewritten to work with IAsyncEnumerable.

Loggers - Simple logging framework for times when a full featured logger is overkill. It can be expanded as needed with additional ILoggers.
