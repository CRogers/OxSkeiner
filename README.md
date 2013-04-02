OxSkeiner
=========

Help Oxford win http://almamater.xkcd.com/! Simply download the 64 or 32 bit program and run it (make sure you get the right one! The Skein hash is highly optimised for 64-bit).

 * __Latest 64-bit release__: https://github.com/CRogers/OxSkeiner/raw/releases/OxSkeiner64-V2.exe
 * __Latest 32-bit release__: https://github.com/CRogers/OxSkeiner/raw/releases/OxSkeiner32-V2.exe


Mac/Linux
----

 * Make sure you have Mono from http://www.mono-project.com/Main_Page or your distro's repo
 * Open a terminal and run `mono OxSkeiner64.exe`

Mouse Lagging? Computer too slow?
---

You can change the number of threads that OxSkeiner uses by passing a command line arg. Open a terminal/command prompt and run `OxSkeiner64.exe 3` to run with 3 threads for example. It defaults to the number of processors you have on your machine.

Reporting
---

OxSkeiner will automatically report it's best results when it gets them - when you see a block of HTML being printed that the response from the website when reporting.
