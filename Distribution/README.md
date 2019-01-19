Distribution build (for Windows 32/64) is hosted on Google Drive.

note for Mono users: MapView needs to run once and then be restarted for its window to resize properly.

Built against .NET 4.5.1

2019 January 19<br>
not available quite yet.

- option to workaround the transparency issue in Mono
- options to set the selection border's color and width
- verify terrains even when Canceling the TilesetEditor
- reposition a couple of toolbar buttons
- provide more information when a Map's MCD records exceeds 256
- use PNG format for inputting/outputting sprites in PckView
- support terrain/unit/bigobs sprites in PckView (but only if they have a .Tab file)

<br><br>
Previous builds

Built against .NET 3.5

2019 January 7<br>
[MapView2_190107b.7z](https://drive.google.com/file/d/1DJ3sCI-izA3N4SFH_xJ4LaFWCo8DWUuw/view?usp=sharing)

- add UFO1A (small scout) to the default MapTilesets.YML ... note to those who want access to the small scout but don't want to bork their current MapTilesets config: generate a MapTilesets.TPL via the Configurator and copy **type:UFO1A** to their working MapTilesets.YML
- issue a warning if the quantity of allocated Terrain MCD-records exceeds 256. The warning, if applicable, is shown when the Map loads by selecting it in the MapTree or when a Map's descriptor is modified causing the Map to reload (eg. terrains have been added or removed)

2019 January 5<br>
[MapView2_190105.7z](https://drive.google.com/file/d/119IjWH4-Ec5W76sg229IgSGBArPfPtNU/view?usp=sharing)

- Bigob PCK+TAB support for PckView.

2018 December 18<br>
[MapView2_181218.7z](https://drive.google.com/file/d/19vCnjBQvfJbIH13KhwoCS-4ZG_CZFSFn/view?usp=sharing)
