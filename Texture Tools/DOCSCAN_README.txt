REQUIREMENTS
In one directory have the following:
Given Files:
- run_doscan.py
- docscan_args.py
- check_boxes.py
- ImageCombiner.py
- plainHeadstone.jpg (A necessity for the ImageCombiner)
Your Files:
- a bounding box file from the AI model (Optional)
- a folder with your images to be turned into textures.


RUNNING THE TOOL
There are two ways to run the tool:
python ./run_docscan <input_folder> <bounding_box_file>
- Runs a check on the bounding box file to account for certain errors.
- Then runs a background cut based on the bounding box file (Sends output to a folder named "Standard_Output" or "Dynamic_Output" based on whether each input image was marked as a standard upright headstone or not in the bounding box file.
- Then creates textures based on the "Standard_Output" folder. (Sends textures to a folder named "Textures")
python ./run_docscan.py <input_folder>
- Runs a rudimentary background cut on all images. (Send output to a folder named "Output")
- Then attempts to texturize ALL images. (Sends textures to a folder named "Textures")
python ./run_docscan.py images