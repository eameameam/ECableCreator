# Cable System Creator

`ECableCreator` is a Unity tool designed to facilitate the creation and visualization of customizable cable systems within your Unity projects. It comprises two scripts, CableCreator and CableGeoCreator, which together enable the easy setup of dynamic cables or ropes.

![ECableCreator Window](/ECableCreator.png)


## Installation

1. Copy `ECableCreator.cs` and `CableGeoCreator.cs` into your Unity project.

## Usage

Attach Scripts:

Add CableCreator to a GameObject to define the point.
Use the + in CableCreator, adjust the offset list to control the cable curveness.
In CableCreator, modify the resolution to control the cable's curve smoothness.

After you set the curves:

Attach CableGeoCreator to the same GameObject and set the cableDiameter and radialSegments for physical appearance adjustments.
Assign a material to the cableMaterial field in CableGeoCreator to define the look of your cable.
