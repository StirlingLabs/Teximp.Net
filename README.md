![alt text](https://bitbucket.org/Starnick/teximpnet/raw/9837c5c4afd6a18826f570a4437330e0e2f3b906/logo.png "TeximpNet Logo")

**The latest release can be downloaded via  [NuGet](https://www.nuget.org/packages/TeximpNet/).**

## Introduction ##
This is the official repository for **TeximpNet**, a cross-platform .NET wrapper for the [FreeImage](http://freeimage.sourceforge.net/) and [Nvidia Texture Tools](https://github.com/castano/nvidia-texture-tools) libraries. This wrapper combines functionality from both native libraries to provide a single, easy to use API to import, manipulate and export images. The primary motivation is for this library to power (offline) content pipelines to import and process textures for graphics applications, such as compressing them to GPU formats and generating mipmap chains. Although the library can be used at runtime to enable your users to import/export image files just as easily. Please see the [FreeImage website](http://freeimage.sourceforge.net/features.html) for what image formats and features are supported. The managed wrappers try to maintain parity with the features of both native libraries (although some of the FreeImage extended functions are not exposed).

P/Invoke is used to communicate with the C-API of the native libraries. The managed assembly is compiled as **AnyCpu** and the native binaries are loaded dynamically for either 32 or 64 bit applications.

The library is split between two parts, a low level and a high level. The intent is to give as much freedom as possible to the developer to work with the native libraries from managed code.

### Low level ###

* Native methods are exposed via the FreeImageLibrary and NvTextureToolsLibrary singletons.
* Located in the *TeximpNet.Unmanaged* namespace.

### High level ###

* The two key classes are the Surface and Compressor classes. 
    * **Surface** directly corresponds to a FreeImage bitmap. The image data is managed by FreeImage and this class wraps many common FreeImage routines.
    * **Compressor** directly corresponds to the Nvidia Texture compressor. This optionally can use a surface or any other data inputs.
* A **DDS importer/exporter** that does not rely on either library (completely written in C#)
* To be as performant as possible, all image data is stored or used as IntPtrs. There is a MemoryHelper class that can be used to allocate, read, and write data to memory. The wrapper does not do marshaling of image data for you, similar to how System.Drawing.Bitmap works.

## Supported Frameworks ##

The library runs on both **.NET Core** and **.NET Framework**, targeting specifically:

* **.NET Standard 1.3**
* **.NET Framework 4.0**
* **.NET Framework 3.5**

This means the NuGet package is compatible with a **wide range** of applications. When targeting .NET Framework, the package uses a MSBuild targets file to copy native binaries to your application output folder. For .NET Core applications, the native binaries are resolved by the *deps.json* dependency graph automatically.

The library can be compiled on any platform that supports  the DotNet CLI build tools or Visual Studio 2017. There is a single **build-time only** dependency, an IL Patcher also distributed as a cross-platform NuGet package. The patcher requires .NET Core 2.0+ or .NET Framework 4.7+ to be installed on your machine to build.

## Supported Platforms ##

The NuGet package supports the following Operating Systems and Architectures out of the box (located in the *runtimes* folder, under [RID](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog)-specific folders):

* **Windows** 
	* x86, x64 (Tested on Windows 10)
* **Linux**
	* x64 (Tested on Ubuntu 18.04 Bionic Beaver)
* **MacOS**
	* x64 (Tested on MacOS 10.13 High Sierra)

You may have to build and provide your own native binaries for a target platform that is not listed. If the library does not support a platform you are targeting, please let us know or contribute an implementation! The logic to dynamically load the native library is abstracted, so new platform implementations can easily be added.

## Unity Users ##

With the release of version 1.4.0, a Unity plugin replicating the NuGet package is outputted to the build folder. You can simply drag and drop the contents into your Unity project. The plugin utilizes a
runtime initiliazation script to ensure the native binaries are loaded when running in editor or standalone.

## Licensing ##

The library is licensed under the [MIT](https://opensource.org/licenses/MIT) license. This means you're free to modify the source and use the library in whatever way you want, as long as you attribute the original authors. The Nvidia Texture Tools library is also licensed under the MIT license. FreeImage is licensed under its [FreeImage Public License](http://freeimage.sourceforge.net/freeimage-license.txt). Please be kind enough to include the licensing text file (it contains all three licenses).

## Contact ##

Follow project updates and more on [Twitter](https://twitter.com/Tesla3D/).

In addition, check out these other projects from the same author:

[AssimpNet](https://bitbucket.org/Starnick/assimpnet) - A wrapper for the Open Asset Import Library, which is a sister library to this one.

[MemoryInterop.ILPatcher](https://bitbucket.org/Starnick/memoryinterop.ilpatcher) - This is the ILPatcher that is required at build time, it uses Mono.Cecil to inject IL code to improve native interop. The ILPatcher is cross-platform, which enables building of TeximpNet on non-windows platforms.

[Tesla Graphics Engine](https://bitbucket.org/Starnick/tesla3d) - A 3D rendering engine written in C# and the primary driver for developing TeximpNet.