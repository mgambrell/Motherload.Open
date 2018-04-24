This is the main "MTS" engine.
Specifically, it's the build system and open-source parts.

And currently, you're reading the main overview of how to use the engine.

It's required that you construct what I call a "packages" directory. It should contain the following things:

* Motherload.Open - https://github.com/mgambrell/Motherload.Open.git
* Motherload.Switch - https://github.com/mgambrell/Motherload.Switch.git
* BRUTE - https://github.com/SickheadGames/BRUTE.git (folder_refactor branch)
* MonoGame.Switch (https://github.com/mgambrell/MonoGame.Switch) (folder_refactor branch)

You can then find a demo at /Motherload.Switch/MtSharperest/Demos/SwitchDemo

Note the skus directory - this is where we build each platform. There's a brute.bat in there, which creates `.bruted` directory containing a buildable .sln

