# Limeko Engine
A flexible, Open-Source Game Engine made to combine all of the best features into one lightweight stack. Runs Natively on [OpenGL](https://www.opengl.org/), with planned support for [Vulkan](https://www.vulkan.org/).

> [!IMPORTANT]
> Limeko is very early in development, and is not a functional "Game Engine". Core framework is still being made and polished.

# Render-Pipe Presets
What are Render-Pipe Presets (RPPs)?

Limeko runs on *one* Render Pipeline. RPPs are like pre-made configurations that let you decide early on what you want to prioritize.
- Viola: Favors fidelity. Volumetric Fog, SSR, SSAO, and high-definition baked lighting settings the second you create the project.
- Citron: A nice middle-ground. Optimized to run okay on low-end hardware, and run with plenty of headroom on modern computers.
- Lime - The highest performance preset. Made for standalone platforms like Quest, or mobile devices such as tablets or smart-phones.

> [!NOTE]
> Limeko does not have multiple Render Pipelines. Instead, it runs on one very flexible Render Pipeline with lots of toggles and settings.

# Physics
Limeko's native physics engine is [Bepu](https://github.com/bepu/bepuphysics2). Bepu is a lightweight, open-source Physics library, like Nvidia's PhysX.

Bepu was etched into Limeko from as early as the first week of development, meaning it's systems revolve around it, but don't depend on it.
You'll get the best experience Limeko can offer.

# Roadmap
(<i>If you'd like to suggest features, send them [here](https://github.com/violetv0id/Limeko-Engine/issues)!</i>)

### In Progress
- Editor UI
- Render Pipeline Framework
- Material Framework

### Near Future
- Hotloading (Models, Scripts)
- Editor Customization
- Engine-Swap tools
- Render-pipe Presets
- PhysX Integration

### Planned
- Virtual Reality support (PCVR, Standalone, both Single-Pass Instanced & Multi-pass)
- Vulkan Support
- Improved traditional lighting methods
- Volumetric lighting & Fog Volumes
