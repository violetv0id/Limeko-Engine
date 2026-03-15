> [!CAUTION]
> Limeko is currently undergoing a **recode**. Previously working features (Basic rendering, Physics) are either no longer working, no longer implemented, or are unpolished.\
> [Learn More](https://github.com/violetv0id/Limeko-Engine/blob/7183eaa26216d777c41db882e531d240dfe33a03/rc_exp.md)

# Limeko Engine
A flexible, Open-Source Game Engine made to combine all of the best features into one lightweight stack. Runs Natively on [OpenGL](https://www.opengl.org/), with planned support for [Vulkan](https://www.vulkan.org/).

If you'd like to learn more, visit the [Official Website](https://violetv0id.github.io/lunark/limeko.html)!

> [!IMPORTANT]
> Limeko is very early in development, and is not a functional "Game Engine". Core framework is still being made and polished.

# Requirements
*(Linux will be supported, and hopefully MacOS too, but not super soon.)*
- Windows 10 or higher
- Modern-ish GPU (post-2012)
- [Net8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

# Render-Pipe Presets
What are Render-Pipe Presets (RPPs)?

Limeko runs on *one* Render Pipeline. RPPs are like pre-made configurations that let you decide early on what you want to prioritize.
- Viola: Favors prettiness. Start with high-fidelity settings the second you create the project.
- Citron: A nice middle-ground. Optimized to run well on low-end hardware, and run with plenty of headroom on modern computers.
- Lime - The highest performance preset. Built versions of your game will be lightweight and capable of being run on weaker machines.

> [!NOTE]
> Limeko does not have multiple Render-Pipelines. Instead, it runs on one very flexible Pipeline with lots of toggles and settings.\
> Selecting a preset does not limit your Project. All RPPs can technically run on the same hardware.

# Physics
Limeko's native physics engine is [Bepu](https://github.com/bepu/bepuphysics2). Bepu is a lightweight, open-source Physics library, like Nvidia's PhysX.

Bepu was etched into Limeko from as early as the first week of development, meaning it's systems revolve around it, but don't depend on it.
You'll get the best experience Limeko can offer.

# Roadmap
(<i>If you'd like to suggest features, send them [here](https://github.com/ariworks-entertainment/Limeko-Engine/issues)!</i>)

### In Progress
- Render Pipeline Framework
- Material Framework

### Near Future
- Editor UI
- Hotloading (Models, Scripts)
- Editor Customization
- Engine-Swap tools
- Render-pipe Presets

### Planned
- Virtual Reality support (PCVR, Standalone, both Single-Pass Instanced & Multi-pass)
- PhysX Integration
- Vulkan Support
- Improved traditional lighting methods
- Volumetric lighting & Fog Volumes

## Links (In Order)
- OpenGL - https://www.opengl.org/
- Vulkan - https://www.vulkan.org/
- Limeko Website - https://violetv0id.github.io/lunark/Limeko.html
- Microsoft DotNet 8.0 - https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Bepu Physics V2 - https://github.com/bepu/bepuphysics2
- Limeko Feature Request / Issues - https://github.com/ariworks-entertainment/Limeko-Engine/issues
