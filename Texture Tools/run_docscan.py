import os, subprocess
import sys

# Set input path
if len(sys.argv) > 1:
    path = os.getcwd()
    input_path = path + "\\" + sys.argv[1]
else:
    print("NO INPUT FOLDER GIVEN - INPUT FOLDER MUST BE IN SAME DIR")


# Create output folder
if not os.path.exists(path + "\Output"):
    os.makedirs(path + "\Output")

print("RUNNING IMAGE FLATTENING")
for f in os.listdir(input_path):
    subprocess.call(["python", ".\docscan_args.py", input_path, f, "Output"])

# Run image combiner on standard headstone output folder.
subprocess.call(["python", ".\ImageCombiner.py", "Output"])
