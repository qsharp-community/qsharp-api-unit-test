# Unit testing documentation code snippets for Q#

This repo provides a Q# Compiler extension that can strip code samples from API docs and turn them into unit tests.

## Motivation

A short description of the motivation behind the creation and maintenance of the project. This should explain **why** the project exists.

## Build status

Build status of continuous integration i.e. travis, appveyor etc. Ex. - 

[![Build Status](https://travis-ci.org/akashnimare/foco.svg?branch=master)](https://travis-ci.org/akashnimare/foco)
[![Windows Build Status](https://ci.appveyor.com/api/projects/status/github/akashnimare/foco?branch=master&svg=true)](https://ci.appveyor.com/project/akashnimare/foco/branch/master)

## Code style

If you're using any code style like xo, standard etc. That will help others while contributing to your project. Ex. -

[![js-standard-style](https://img.shields.io/badge/code%20style-standard-brightgreen.svg?style=flat)](https://github.com/feross/standard)
 
## Screenshots

Include logo/demo screenshot etc.

## Tech/framework used

Ex. -

<b>Built with</b>
- [Electron](https://electron.atom.io)

## Features

What makes your project stand out?

## Code Example

Show what the library does as concisely as possible, developers should be able to figure out **how** your project solves their problem by looking at the code example. Make sure the API you are showing off is obvious, and that your code is short and concise.

## Installation

Provide step by step series of examples and explanations about how to get a development env running.

## API Reference

Depending on the size of the project, if it is small and simple enough the reference docs can be added to the README. For medium size to larger projects it is important to at least provide a link to where the API reference docs live.

## Tests
Describe and show how to run the tests with code examples.

## How to use?
If people like your project they’ll want to learn how they can use it. To do so include step by step guide to use your project.

## Contribute

Let people know how they can contribute into your project. A [contributing guideline](https://github.com/zulip/zulip-electron/blob/master/CONTRIBUTING.md) will be a big plus.

## Credits
Give proper credits. This could be a link to any repo which inspired you to build this project, any blogposts or links to people who contrbuted in this project. 

#### Anything else that seems useful

## License
A short snippet describing the license (MIT, Apache etc)

MIT © [Q# Community](qsharp.community)






# Creating a NuGet package containing a Q# compiler extension

This project contains a template for packaging a Q# compiler extension. For more information about Q# compiler extensions see [here](https://github.com/microsoft/qsharp-compiler/tree/master/src/QuantumSdk#extending-the-q-compiler). For more information on NuGet packages, see [here](https://docs.microsoft.com/en-us/nuget/what-is-nuget).

Prerequisites: [NuGet tools](https://docs.microsoft.com/en-us/nuget/install-nuget-client-tools)

To create the package, follow the following steps:
- From within the project directory, run `dotnet build`.
- From within the project directory, run `nuget pack`.
- Copy the created .nupkg file into you [local NuGet folder](https://docs.microsoft.com/en-us/nuget/hosting-packages/local-feeds).
- You can now use that package like any other NuGet package. 

In order to use the created package as a Q# compiler extension when building a Q# project, add the following package reference to your project file:
```
    <PackageReference Include="CustomExtension.Package" Version="1.0.0" IsQscReference="true" />
```
The extension will only be included in the build process if`IsQscReference` is set to `true`. For more information, see this [readme](https://github.com/microsoft/qsharp-compiler/blob/master/src/QuantumSdk/README.md). 