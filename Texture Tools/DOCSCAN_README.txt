REQUIREMENTS
In one directory have the following:
Given Files:
- Run_texture_creator.ipynb
- run_doscan.py
- docscan_args.py
- check_boxes.py
- ImageCombiner.py
- plainHeadstone.jpg (A necessity for the ImageCombiner)

Your Files:
- A folder named "images" with your MASKED images to be turned into textures. (The masked image must use pure black to and result in a headstone silhouette that bounds a skewed rectangle/square)


RUNNING THE TOOL
There are two ways to run the tool:

1. Run the following command indicated after the arrow in windows command prompt ------> python ./run_docscan images
	-This is useful for debugging as the code will print the process of automated texturing.

2. Or run the Python Journal "Run_texture_creator" by opening the file and following the specified instructions for either Windows or MAC PCs

*******************************************************************************************************************

The process starts off by creating an "output" folder with flattened photos. 
This is done so the user can look for photos with any defects so they can be reprocessed after changes are made. 
The "finalTexture" folder contains all final textures that can be used in a 3D environment.