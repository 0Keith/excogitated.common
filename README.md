# excogitated.common
This is a library that I've built over the last decade of software development. Most classes were created to simplify development and reduce code. I use this library in most of the projects I work on and if anyone else finds it useful, great.

Currently working on documentation and test cases for the contained classes. Feels like that might take longer than it took to actually write them. Until that is completed it'll remain in an alpha state.

Notable classes:

Module - Micro IOC framework using static generic fields. Should be faster than any other solution available but has an extremely limited feature set.
  
AsyncQueue - Created before IAsyncEnumerable was available but still useful for multiple producer and or multiple consumer scenarios.
  
JsonFileStore - A simple object "database". Each entry is saved as a compressed json file and loaded into memory on first access. Saving and loading backups to an auxillary file location is supported. I was annoyed with storage limits while using Mongo Atlas when I decided to create this. My use case was extremely simple, the stored data was 5gb+, but compressed it is less than 200mb. Didn't feel that paying $50+ a month for 5gb of storage would be money well spent.
  
Extensions_AsyncEnumerable - Most of the Linq extensions available for IEnumerable rewritten to work with IAsyncEnumerable.

Loggers - Simple logging framework for times when a full featured logger is overkill. It can be expanded as needed with additional ILoggers.
