# ExifToolWrapper
A C# Wrapper for Phil Harvey's Excellent ExifTool

## About ExifTool
[ExifTool](https://sno.phy.queensu.ca/~phil/exiftool/) is a command-line application that can retrieve and update metadata in media files produced by cameras and other devices. It originally handled [EXIF](https://en.wikipedia.org/wiki/Exif) data embedded in .jpg files produced by digital cameras. As Harvey has improved the tool over the years it now processes embedded metadata in a variety of formats — principally those produced by digital cameras and video recorders. 

## About ExifToolWrapper
ExifToolWrapper is a *CodeBit* written in c# that loads ExifTool and calls it to retrieve metadata from media files. A future version may include the ability to set metadata properties. The purpose is to make the features of ExifTool conveniently usable from .NET applications.

## About CodeBit
A CodeBit is a way to share common code that's lighter weight than [NuGet](http://nuget.org). CodeBits are contained in one source code file. A structured comment at the beginning of the file indicates where to find the master copy so that automated tools can retrieve and update CodeBits to the latest version. For more information see [http://FileMeta.org/CodeBit.html](http://FileMeta.org/CodeBit.html).

In this case, the CodeBit is the ExifToolWrapper.cs file. It is intended to be reused in other applications.

## About the Project
This project includes the ExifToolWrapper.cs CodeBit and test code to make sure the class functions correctly. It was created in Microsoft Visual Studio 2017 and uses .NET Framework 4.0.
