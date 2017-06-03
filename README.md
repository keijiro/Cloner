Cloner
======

![gif](http://i.imgur.com/jFljvB3.gif)
![gif](http://i.imgur.com/dtPSzKW.gif)
![screenshot](http://i.imgur.com/LpWU8lZm.png)

**Cloner** is an example of use of the [procedural instancing] feature that
was newly introduced in Unity 5.6.

*Cloner* creates instances of a given template mesh and place them onto
vertices of a base model. It uses a [compute shader] for vertex animation
and [GPU instancing] for duplicating the template mode. With helps of these
GPU features, it draws complex animation without spending much CPU time.

[procedural instancing]: https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstancedIndirect.html
[compute shader]: https://docs.unity3d.com/Manual/ComputeShaders.html
[GPU instancing]: https://docs.unity3d.com/Manual/GPUInstancing.html

System Requirements
-------------------

- Unity 5.6 or later

*Cloner* only runs on the platforms that support compute shaders and GPU
instancing.

License
-------

[MIT](LICENSE.md)
