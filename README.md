# excogitated.common
Currently working on documentation and test cases for the contained classes. Feels like that might take longer than it took to actually write them.

Notable classes:

Module<T> - Micro IOC framework using static generic fields. Should be faster than any other solution available but has an extremely limited feature set.
  
AsyncQueue<T> - Created before IAsyncEnumerable<T> was available but still useful.  
  
JsonFileStore<T> - A simple object "database". Each entry is saved as a compressed json file and loaded into memory on first access. Saving and loading backups to an auxillary file location is supported. I was annoyed with storage limits while using Mongo Atlas when I decided to create this. My use case was extremely simple, the stored data was 5gb+, but compressed it is less than 200mb. Didn't feel that warranted paying $50+ a month for a personal project.
  
Extensions_AsyncEnumerable - Most of the Linq extensions available for IEnumerable<T> configured to work with IAsyncEnumerable<T>.
