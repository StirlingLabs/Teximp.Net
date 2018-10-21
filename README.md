![alt text](https://bitbucket.org/Starnick/teximpnet/raw/9837c5c4afd6a18826f570a4437330e0e2f3b906/logo.png "TeximpNet Logo")

**The latest release can be downloaded via  [NuGet](https://www.nuget.org/packages/TeximpNet/).**

## Introduction ##
This is the official repository for **TeximpNet**, a cross-platform .NET wrapper for the [FreeImage](http://freeimage.sourceforge.net/) and [Nvidia Texture Tools](https://github.com/castano/nvidia-texture-tools) libraries. This wrapper combines functionality from both unmanaged libraries to provide a single, easy to use API surface to import, manipulate and export images. The general motivation is to process textures for graphics applications, so there is an emphasis on using the Nvidia Texture Tools library for compression and mipmap chain generation. 

P/Invoke is used to communicate with the C-API's of both native libraries. The managed assembly is compiled as AnyCpu and the native DLLs are loaded dynamically for either 32 or 64 bit applications.

The library is split between two parts, a low level and a high level. The intent is to give as much freedom as possible to the developer to work with the native libraries from managed code.

### Low level ###

* Native methods are exposed via the FreeImageLibrary and NvTextureToolsLibrary singletons.
* Located in the *TeximpNet.Unmanaged* namespace.

### High level ###

* The two key classes are the Surface and Compressor classes. 
    * Surface directly corresponds to a FreeImage bitmap. The image data is managed by FreeImage and this class wraps many common FreeImage routines.
    * Compressor directly corresponds to the Nvidia Texture compressor. This optionally can use a surface or any other data inputs.
* To be as performant as possible, all image data is stored or used as IntPtrs. There is a MemoryHelper class that can be used to allocate, read, and write data to memory. The wrapper does not do marshaling of image data for you, similar to how System.Drawing.Bitmap works.

## Supported Platforms ##

TeximpNet officially targets the **.NET Standard 1.3** and supplies binaries for the following platforms:

* **Windows x86/x64**
* **Linux x64** (Ubuntu 18.04)
* **MacOS x64** (MacOS 10.13)

If your preferred platform is not listed, you will have to build and supply the native binaries yourself. Please consult the *VersionList.txt* file in the **libs** folder for details on what version the library expects. If the library does not work on your platform, please let us know so we can try and get **TeximpNet** running on it!

For legacy applications, the NuGet package also has targets **.NET Framework 4.x** and **.NET Framework 3.5** should you need them. The project is compiled with Visual Studio 2017 on windows, and the DotNet CLI for non-Windows platforms. There is one **build-time only** dependency, an IL Patcher also distributed as a cross-platform NuGet package. As long as you're
able to build with Visual Studio or the DotNet CLI, the library *should* compile without issue on any platform.

## Licensing ##

The library is licensed under the [MIT](https://opensource.org/licenses/MIT) license. This means you're free to modify the source and use the library in whatever way you want, as long as you attribute the original authors. The Nvidia Texture Tools library is also licensed under the MIT license. FreeImage is licensed under its [FreeImage Public License](http://freeimage.sourceforge.net/freeimage-license.txt). Please be kind enough to include the licensing text file (it contains all three licenses).

## Contact ##

Follow project updates and more on [Twitter](https://twitter.com/Tesla3D/).

In addition, check out these other projects from the same author:

[AssimpNet](https://bitbucket.org/Starnick/assimpnet) - A wrapper for the Open Asset Import Library, which is a sister library to this one.

[MemoryInterop.ILPatcher](https://bitbucket.org/Starnick/memoryinterop.ilpatcher) - This is the ILPatcher that is required at build time, it uses Mono.Cecil to inject IL code to improve native interop. The ILPatcher is cross-platform, which enables building of TeximpNet on non-windows platforms.

[Tesla Graphics Engine](https://bitbucket.org/Starnick/tesla3d) - A 3D rendering engine written in C# and the primary driver for developing TeximpNet.