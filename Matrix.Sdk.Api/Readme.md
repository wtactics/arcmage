This net5 Matrix.Sdk is a fork from https://github.com/VRocker/MatrixAPI

Following changes were made in order to make it a pure .net 5 library without the need for UWP.
- Upgraded to .net5, made namespaces uniform
- Merged all MatrixAPI partial classes into one
- Removed custom MatrixSpec attribute class
- Removed dependency on UWP (Windows Timers)
- Added option to send matrix formatted text (html) 
- Fixed null pointer expceptions in ParseClientSync
- Standardized property naming

The Matrix.Sdk is under Apache License, Version 2.0, January 2004, http://www.apache.org/licenses/