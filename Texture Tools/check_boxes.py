import os, subprocess
import csv
import sys

#Only run this script if passed a value.
if(len(sys.argv)>1):
    bounding_boxes_file_path = sys.argv[1]
    rec_path = os.getcwd()+'\\'+bounding_boxes_file_path
    f = open("boxes.txt", "w")

if os.path.isfile(rec_path):
    with open(rec_path) as file:
        
        tsv_file = csv.reader(file, delimiter="\t")
        for line in tsv_file:
            s = line[1].split(",")
            if(s[2]>s[3]): #If width larger than height set to Personal (or Grass?)
                f.write(line[0]+"\t"+line[1]+"\t"+"3\n")
            else:
                f.write(line[0]+"\t"+line[1]+"\t"+line[2]+"\n")
f.close()
