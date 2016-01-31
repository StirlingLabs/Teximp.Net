**This library is still under development, a binary download and nuget package will be available when it stabilizes.**

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

The library currently only supports Windows officially. It has a **Linux** and **Mac** implementation to load and communicate with the native library for those platforms, but you have to provide the native binary yourself.

The library is compiled using Visual Studio 2015 and at runtime has no other external dependencies other than the native libraries. However, there is a compile time dependency using [Mono.Cecil](https://github.com/jbevain/cecil/). If you compile without using the VS projects/MSBuild environment, the **only** special instruction is that you need to ensure that the interop generator patches the TeximpNet.dll in a post-build process, otherwise the library won't function correctly. This is because Mono.Cecil is used to inject IL into the assembly to make interoping with the native library more efficient.

## Licensing ##

The library is licensed under the [MIT](https://opensource.org/licenses/MIT) license. This means you're free to modify the source and use the library in whatever way you want, as long as you attribute the original authors. The Nvidia Texture Tools library is also licensed under the MIT license. FreeImage is licensed under its [FreeImage Public License](http://freeimage.sourceforge.net/freeimage-license.txt). Please be kind enough to include the licensing text file (it contains both licenses).

## Contact ##

Follow project updates and more on [Twitter](https://twitter.com/Tesla3D/).

In addition, check out these other projects from the same author:

[AssimpNet](https://bitbucket.org/Starnick/assimpnet) - A wrapper for the Open Asset Import Library, which is a sister library to this one.

[Tesla Graphics Engine](https://bitbucket.org/Starnick/tesla3d) - A 3D rendering engine written in C# and the primary driver for developing TeximpNet.