import cv2
import numpy as np
import sys


def cut(img, rectangle):
    # Setup for GrabCut - Always constant
    mask = np.zeros(img.shape[:2], dtype=np.uint8)
    background_model = np.zeros((1, 65), np.float64)
    foreground_model = np.zeros((1, 65), np.float64)
    # Make the mask
    (mask, background_model, foreground_model) = cv2.grabCut(
        img,
        mask,
        rectangle,
        background_model,
        foreground_model,
        15,
        cv2.GC_INIT_WITH_RECT,
    )
    # Set all background objects to zero and all other objects to one
    outputMask = np.where((mask == cv2.GC_BGD) | (mask == cv2.GC_PR_BGD), 0, 1)
    # Scale the mask
    outputMask = (outputMask * 255).astype("uint8")
    # Apply the mask
    img = cv2.bitwise_and(img, img, mask=outputMask)

    return img


def edge(img):
    # Make the image grayscale so edge detection can be performed
    grayscale_img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    grayscale_img = cv2.GaussianBlur(grayscale_img, (1, 1), 0)
    # Edge detection
    canny = cv2.Canny(grayscale_img, 0, 200)
    canny = cv2.dilate(canny, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (80, 80)))

    return canny


def contour(img, canny, num, num_contours):
    # Make blank image
    blank_img = np.zeros_like(img)
    # Finding contours for the detected edges.
    contours, hierarchy = cv2.findContours(
        canny, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE
    )
    for contour in contours:
      contour=cv2.convexHull(contour)
    # Keeping only the largest detected contour.
    page = sorted(contours, key=cv2.contourArea, reverse=True)[:num_contours]
    con = cv2.drawContours(blank_img, page, -1, (0, 255, 255), 3)
    return con, page


def corners(img, page, num):
    # Blank canvas.
    con = np.zeros_like(img)
    corners = []
    # Loop over the contours.
    for c in page:
        # Approximate the contour
        for ApproxFactor in range(5):
            if len(corners) == 4:
                break
            epsilon = 0.005 * ApproxFactor * cv2.arcLength(c, True)
            corners = cv2.approxPolyDP(c, epsilon, True)
            # If our approximated contour has four points
            
            if len(corners) <= 3:
                print("Error too little corners for image")
                break
        if len(corners) == 4:
            cv2.drawContours(con, c, -1, (0, 255, 255), 3)
            cv2.drawContours(con, corners, -1, (0, 255, 0), 10)
            break

    # Sorting the corners and converting them to desired shape.
    corners = sorted(np.concatenate(corners).tolist())

    corners = order_points(corners)

    # Displaying the corners.
    for index, c in enumerate(corners):
        character = chr(65 + index)
        cv2.putText(
            con,
            character,
            tuple(c),
            cv2.FONT_HERSHEY_SIMPLEX,
            1,
            (255, 0, 0),
            1,
            cv2.LINE_AA,
        )

    return con, corners


def order_points(pts):
    # Rearrange coordinates to order: top-left, top-right, bottom-right, bottom-left
    rect = np.zeros((4, 2), dtype="float32")
    pts = np.array(pts)
    s = pts.sum(axis=1)
    # Top-left point will have the smallest sum.
    rect[0] = pts[np.argmin(s)]
    # Bottom-right point will have the largest sum.
    rect[2] = pts[np.argmax(s)]

    diff = np.diff(pts, axis=1)
    # Top-right point will have the smallest difference.
    rect[1] = pts[np.argmin(diff)]
    # Bottom-left will have the largest difference.
    rect[3] = pts[np.argmax(diff)]

    # Return the ordered coordinates.
    return rect.astype("int").tolist()


def get_coords(corners):
    (tl, tr, br, bl) = corners
    # Finding the maximum width.
    widthA = np.sqrt(((br[0] - bl[0]) ** 2) + ((br[1] - bl[1]) ** 2))
    widthB = np.sqrt(((tr[0] - tl[0]) ** 2) + ((tr[1] - tl[1]) ** 2))
    maxWidth = max(int(widthA), int(widthB))
    # Finding the maximum height.
    heightA = np.sqrt(((tr[0] - br[0]) ** 2) + ((tr[1] - br[1]) ** 2))
    heightB = np.sqrt(((tl[0] - bl[0]) ** 2) + ((tl[1] - bl[1]) ** 2))
    maxHeight = max(int(heightA), int(heightB))
    # Final destination co-ordinates.
    destination_corners = [[0, 0], [maxWidth, 0], [maxWidth, maxHeight], [0, maxHeight]]
    return destination_corners


# Given corners of the tombstone output a version that pads the top of the corners with perpsective
def padTopCorners(corners):
    #'t' means top corners 'r' means a corner on the right side 'b' means bottom 'l' means left side
    (tl, tr, br, bl) = corners
    tl = np.array(tl)
    tr = np.array(tr)
    br = np.array(br)
    bl = np.array(bl)

    # Create vectors from the left and right contour lines  to create new contours with respect to perspective
    leftVector = abs(tl - bl)
    rightVector = abs(tr - br)

    # Get the magnitude
    leftMagnitude = np.linalg.norm(leftVector)
    rightMagnitude = np.linalg.norm(rightVector)

    # Normlaize the vectors
    normalLeftVector = leftVector / leftMagnitude
    normalRightVector = rightVector / rightMagnitude

    # Pad the magnitude by 10% and add the vector coordinates to the coordinates of the old top corners
    newTLCorner = -(normalLeftVector * 0.1 * leftMagnitude) + tl
    newTRCorners = -(normalRightVector * 0.1 * rightMagnitude) + tr
    return (newTLCorner, newTRCorners)


def perspective(img, obj_corners, dest_corners):
    print("Flattening image...")
    M = cv2.getPerspectiveTransform(np.float32(obj_corners), np.float32(dest_corners))
    # Perspective transform using homography.
    final = cv2.warpPerspective(
        orig_img, M, (dest_corners[2][0], dest_corners[2][1]), flags=cv2.INTER_LINEAR
    )
    return final


# Automatically areas of pure black
def cropBlack(img):
    gray_img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    coords = cv2.findNonZero(gray_img)
    if coords is None:
        return img
    # Get bounds for non black areas
    x, y, w, h = cv2.boundingRect(coords)
    # Assign cropepd image
    cropped_img = img[y : y + h, x : x + w]
    return cropped_img


# ---------------------MAIN---------------------
# ---SET STARTING VARIABLES---
# Get the image from the input directory.
input_path = str(sys.argv[1])
input_file = str(sys.argv[2])
input_path = input_path + "\\" + input_file
orig_img = cv2.imread(input_path, 1)
print("Processing image: ", input_file)
# Set output folder
output_path = sys.argv[3]

# Set corner detection type
dest_corner_type = "Other"
if len(sys.argv) > 4:
    dest_corner_type = sys.argv[4]


# ---FIRST BACKGROUND CUT---
# Close and dilate image

orig_img = cv2.copyMakeBorder(
    orig_img, 100, 100, 100, 100, cv2.BORDER_CONSTANT, value=0
)
img = orig_img.copy()
img[img > 10] = 255
img[img < 10] = 0


# Perform edge/contour detectiong then corner detection.
canny = edge(img)
con, page = contour(img, canny, 1, 1)
img, img_corners = corners(con, page, 1)
padded_top_corners = padTopCorners(img_corners)

# Pad top conrers
img_corners[0:2] = padded_top_corners[0:2]


# ---PERSPECTIVE CHANGE---
# Sets up the corners to have a good perspective.
dest_corners = get_coords(img_corners)

# Overwrites that good perspective to instead fit the required lenght by width.
# Front headstone dimensions: 1005 x 1590
# Back headstone dimensions: 1043 x 1590
if dest_corner_type == "Standard":
    dest_corners = [[0, 0], [1005, 0], [1005, 1590], [0, 1590]]

# Perform image perspective changes
img_result = perspective(orig_img, img_corners, dest_corners)

# Crop areas off pure black
img_result = cropBlack(img_result)
img_result=cv2.copyMakeBorder(img_result,100,100,100,100, cv2.BORDER_CONSTANT,value=[0,0,0])

# ---OUTPUT---
# Output the image to the current directory.
cv2.imwrite(output_path + "/" + (input_file), img_result)
