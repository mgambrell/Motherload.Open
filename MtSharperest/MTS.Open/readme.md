This is the main "MTS" engine.
Specifically, it's the build system and open-source parts.

And currently, you're reading the main overview of how to use the engine.

It's required that you construct what I call a "packages" directory. It should contain the following things:

* Motherload.Open - https://github.com/mgambrell/Motherload.Open.git
* Motherload.Switch - https://github.com/mgambrell/Motherload.Switch.git
* BRUTE - https://github.com/SickheadGames/BRUTE.git (folder_refactor branch)
* MonoGame.Switch (https://github.com/mgambrell/MonoGame.Switch) (matts_static_pinvokes branch)

You can then find a demo at /Motherload.Switch/MtSharperest/Demos/SwitchDemo

Note the skus directory - this is where we build each platform. There's a brute.bat in there, which creates a `.bruted` directory containing a buildable .sln. Run my brute.bats from a visual studio command prompt (I'm using vs2015).

When opening a demo or template, change visual studio to select the Debug|x64 configuration. This is where you'll do the bulk of the .net-based prototyping for your game. (TODO: consider renaming this to Proto and Brute, for a more prescriptive arrangement)

To begin a project, you are probably best off copying a demo or template. However, I'll tell you what's important to know for orientation reasons if nothing else. The projects supplied with MTS will be following these rules and it will be hard to fight with them.

The sln and c# csprojs should have a Brute configuration. This should define the BRUTED symbol and otherwise be like a release configuration. Whether it's best to set 'optimize' here is philosophical.

These should be targeting the x64 platform. Everything should be targeting x64. Don't use AnyCpu -- my demos, and possibly less optional pieces of the build system depend on a `bin\x64\configuration` output instead of a `bin\configuration` output.

Since BRUTED is defined, certain features of the engine will be disabled or stubbed, which assist with prototyping on the PC (largely, the content hotloading system). I could have engineered things so this wasn't necessary... but why not try this?

Therefore, to make a bruted console project, we'll need to run brute on a Brute|x64 build configuration output. The build scripts I've provided take care of building that for you; so just do your prototyping in Debug|x64 and do the bruting to check the results on console.

When adding libraries to your sln, be sure to add the associated native project. But do not select it to build in the Brute|x64 configuration -- the bruted output will take care of building it.

When making a new sln, be sure to reference MTS.Engine.Pipeline. Nothing needs it directly, but it needs to be built and output so your Proto target can hotload content.

TODO: add more prescriptive docs, organized by task: Try a new project; make a console build; add an existing library; develop a new library

