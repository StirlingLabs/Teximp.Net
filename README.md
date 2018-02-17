**The latest release can be downloaded from the [Downloads](https://bitbucket.org/Starnick/teximpnet/downloads) section or via [NuGet](https://www.nuget.org/packages/TeximpNet/).**

## Introduction ##
This is the official repository for **TeximpNet**, a cross-platform .NET wrapper for the [FreeImage](http://freeimage.sourceforge.net/) and [Nvidia Texture Tools](https://github.com/castano/nvidia-texture-tools) libraries. This wrapper combines functionality from both unmanaged libraries to provide a single, easy to use API surface to import, manipulate and export images. The general motivation is to process textures for graphics applications, so there is an emphasis on using the Nvidia Texture Tools library for compression and mipmap chain generation. 

P/Invoke is used to communicate with the C-API's of both native libraries and because the managed assembly is compiled as **AnyCpu** where the native DLLs are loaded dynamically, **TexImpNet** fully supports usage with 32 and 64 bit applications without needing to be recompiled.

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

TeximpNet officially targets the **.NET Standard 1.3** and supplies binaries for **32/64 bit Windows** and **64 bit Linux (tested on ubuntu)**. The library is able to support **MacOS** but native binaries are not yet bundled with the official NuGet package. To use the library on your
preferred platform, you may have to build and supply the native binaries yourself.

Additionally, the NuGet package has targets for **.NET Framework 4.x** and **.NET Framework 3.5** should you need them. It was compiled with Visual Studio 2017, but it has been compiled on Ubuntu using the DotNet CLI. There is one **build-time only** dependency, an IL Patcher also distributed as a cross-platform NuGet package. As long as you're
able to build with Visual Studio or the DotNet CLI, the library *should* compile without issue on any platform.

## Licensing ##

The library is licensed under the [MIT](https://opensource.org/licenses/MIT) license. This means you're free to modify the source and use the library in whatever way you want, as long as you attribute the original authors. The Nvidia Texture Tools library is also licensed under the MIT license. FreeImage is licensed under its [FreeImage Public License](http://freeimage.sourceforge.net/freeimage-license.txt). Please be kind enough to include the licensing text file (it contains both licenses).

## Contact ##

Follow project updates and more on [Twitter](https://twitter.com/Tesla3D/).

In addition, check out these other projects from the same author:

[AssimpNet](https://bitbucket.org/Starnick/assimpnet) - A wrapper for the Open Asset Import Library, which is a sister library to this one.

[MemoryInterop.ILPatcher](https://bitbucket.org/Starnick/memoryinterop.ilpatcher) - This is the ILPatcher that is required at build time, it uses Mono.Cecil to inject IL code to improve native interop. The ILPatcher is cross-platform, which enables building of TeximpNet on non-windows platforms.

[Tesla Graphics Engine](https://bitbucket.org/Starnick/tesla3d) - A 3D rendering engine written in C# and the primary driver for developing TeximpNet.