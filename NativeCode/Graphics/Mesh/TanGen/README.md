# TGen

This is a very basic tangent generator, written in C++.
The main purpose of this project is to facilitate adoption of, and discussion about, the proper setup of tangent spaces for glTF 2.0 assets.

Current Features:
* Generation of per-corner tangents for triangle data with UVs
* Computation of per-wedge / per-UV-vertex tangent spaces
* Tangent frame orthogonalization
* Encoding of 4-component tangents (with "flip factor") for avoiding explicit binormals
* Simple C++ implementation, no dependencies

The code consists basically of one header + .cpp file.
For debugging and visualization, there is also a simple X3D exporter in a separate file, which was used to generate the 3D visualizations shown below.
The baked tangent-space normal maps are just provided for demonstration purposes, the actual baking code is not part of this repository.

So far, the C++ code from this project has just been compiled and tested with VS 2015.

Feedback and contributions are always welcome. 


## Results

These are some basic results - images show tangent frames, detail mesh, and resulting baked normal map.

### Landscape
<div style="width:100%">
<a href="https://mlimper.github.io/tgen/demo/landscape/index.html">        <img src="images/landscape-tangents.jpg"  width="256"></a>
<a><img src="images/landscape-details.jpg"  width="256"></a>
<a href="https://mlimper.github.io/tgen/demo/landscape/baked/NormalsTS.png"><img src="images/landscape-normalmap.jpg" width="256"></a>
</div>

[Web Demo](https://mlimper.github.io/tgen/demo/landscape/index.html)


### Victor
<div>
<a href="https://mlimper.github.io/tgen/demo/victor/index.html">         <img src="images/victor-tangents.jpg"  width="256"></a>
<a><img src="images/victor-details.jpg"  width="256"></a>
<a href="https://mlimper.github.io/tgen/demo/victor/baked/NormalsTS.png"><img src="images/victor-normalmap.jpg" width="256"></a>
</div>

[Web Demo](https://mlimper.github.io/tgen/demo/victor/index.html)
