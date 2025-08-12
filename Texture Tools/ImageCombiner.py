import os
import cv2
import re
import sys
import numpy as np


def find_overlay_images(directory):
    """
    Finds and pairs F and R images within a directory.
    Returns a list of tuples: (F image path, R image path (None if not present), file name for output).
    """

    overlay_images_info = []
    images = os.listdir(directory)
    f_images = [img for img in images if img.endswith("-F.jpg")]

    for f_img_name in f_images:
        # Extract the file name according to the new specification
        match = re.search(r"P([A-E]|MA)-0-(\d{1,3})(-?[A-E]?)", f_img_name)
        if match:
            # Construct new file name, ensuring no extra dash is included
            extra_letter = match.group(3) if match.group(3) != "-" else ""
            file_name = f"{match.group(1)}-{match.group(2)}{extra_letter}-F"
        else:
            file_name = f_img_name[:-6]
            print(
                f"Could not parse filename {f_img_name}, using {file_name} as base name."
            )

        r_img_name = f_img_name.replace("-F.jpg", "-R.jpg")
        r_img_path = (
            os.path.join(directory, r_img_name) if r_img_name in images else None
        )
        f_img_path = os.path.join(directory, f_img_name)
        overlay_images_info.append((f_img_path, r_img_path, file_name))

        print(f"Matching: {f_img_name} {'with ' + r_img_name if r_img_path else ''}")

    return overlay_images_info


# Scales the images down so the width of the image matches the resolution
def resize_with_bounds(img, bounds):
    """
    Given a BGR image and bounds defined by (height and width) this function resizes an image to best fit in the bounds while also
    preserving the proportions. The height must never be bigger than the texture resolution (Should be 1024 px by 1024 px)
    and the width must not be bigger than half of the texture resoltion

    Returns:
        Resized BGR image
    """
    # (height,width)
    (h, w) = bounds

    # The shape of an image is ordered by height then width, the array reverses it temporarily
    img_proportions = np.array([img.shape[1], img.shape[0]])
    img_proportions.astype(float)

    # Adjust to the width of the bounds
    img_proportions = img_proportions * (w / img_proportions[0])
    # If the new image dimensions is too tall scale it down
    if img_proportions[1] > h:
        img_proportions = img_proportions * (h / img_proportions[1])

    # Convert the dimensions to an integer
    img_proportions = (img_proportions[:]).astype(int)

    # Resize cv2.resize requires the dimensions to be ordered by width then height
    img_result = cv2.resize(img, (img_proportions[0], img_proportions[1]))
    # Return the resized photo
    return img_result


def is_shadowed_image(img):
    """
        If the image is dark return true, else return false
        
    """
    img_copy=img.copy()
    gray_img=cv2.cvtColor(img_copy,cv2.COLOR_BGR2GRAY)
    
    if(np.average(gray_img[gray_img>15])<150):
        print("------------Fixing dark photo---------------")
        return True
    return False


def auto_balance_brightness_with_boost(image, max_gamma,brightness_threshold=80 ):
    """
    Increase the contrast of the given BGR image using CLAHE and increase brightness using a threshould
    The threshold defines the brightest a dark can be to be edited. This method ignores areas of pure black

    Returns:
        A brightened BGR image
    """
    # Convert to YUV and extract Y (brightness) channel
    img_yuv = cv2.cvtColor(image.copy(), cv2.COLOR_BGR2YUV)
    
    y_channel = img_yuv[:, :, 0]

    # Create mask to ignore pure black pixels
    non_black_mask = np.any(image != [0, 0, 0], axis=2)

    # Use CLAHE to enhance contrast
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    y_clahe = clahe.apply(y_channel)

    # Replace only non-black regions
    y_channel_out = y_channel.copy()
    y_channel_out[non_black_mask] = y_clahe[non_black_mask]

    # Check mean brightness after CLAHE (excluding pure black)
    mean_brightness = np.mean(y_channel_out[non_black_mask])

    # If too dark, apply gamma correction to brighten
    if mean_brightness < brightness_threshold:
        gamma = min(max_gamma, brightness_threshold / (mean_brightness + 1e-5))
        inv_gamma = 1.0 / gamma
        # Build lookup table
        table = np.array([(i / 255.0) ** inv_gamma * 255 for i in range(256)]).astype(
            "float32"
        )
        # Apply gamma correction in BGR space for brightness shift
        image = cv2.LUT(image, table)

        # Recompute YUV after gamma correction
        img_yuv = cv2.cvtColor(image, cv2.COLOR_BGR2YUV)
        y_channel_out = img_yuv[:, :, 0]

    # Insert modified Y channel and convert back to BGR
    img_yuv[:, :, 0] = y_channel_out
    balanced_img = cv2.cvtColor(img_yuv, cv2.COLOR_YUV2BGR)

    # Enforce black mask
    balanced_img[~non_black_mask] = [0, 0, 0]
    balanced_img[balanced_img>255]=255
    return balanced_img



def texture_blend(overlay,background,black_mask_value=10):
    
    upper_bound = np.array([black_mask_value,black_mask_value,black_mask_value])
    lower_bound = np.array([0,0,0])
    #Get indicies where black value occurs
    mask = cv2.inRange(overlay, lower_bound, upper_bound)

    masked_image= overlay.copy()
    ROI=cv2.cvtColor(masked_image, cv2.COLOR_BGR2GRAY)
    ROI[mask!=0]=0
    ROI[mask==0]=255
    #hole_fill_kernel=cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (25, 25))
    ROI[mask!=0]=255
    ROI[mask==0]=0
    #ROI = cv2.morphologyEx(ROI, cv2.MORPH_CLOSE, hole_fill_kernel)
    ksize=25
    kernel=np.ones((ksize,ksize),np.uint8)
    ROI=cv2.dilate(ROI,kernel)
    blurred_img=overlay.copy()
    
    blurred_img=cv2.GaussianBlur(blurred_img,(ksize,ksize),5,5)
    
    blurred_ROI=cv2.GaussianBlur(ROI,(ksize,ksize),5,5)
    # Put the normal tombstone photo on top of the blurred version
    blended_overlay=blurred_img.copy()
    blended_overlay[mask==0]=overlay[mask==0]
    blurred_ROI=cv2.cvtColor(blurred_ROI,cv2.COLOR_GRAY2BGR)
    blurred_ROI=np.array(blurred_ROI)
    
    #Use opacity Factor to blend according to the blurred Region of Interest
    opacity_factor=blurred_ROI/255.0
    background=background.astype(np.float32)
    blended_overlay=blended_overlay.astype(np.float32)
    final_image=((1-opacity_factor)*blended_overlay) + ((opacity_factor)*(background))
    #final_image=oerlay 
    final_image[final_image>254]=255
    return final_image.copy()


def combine_images(
    background_path,
    overlay_images_info,
    output_directory,
    positions_and_sizes,
    resolution,
):
    """
    Combines F and R images (if available) with a background image according to specified positions and sizes.
    Saves the output to jpg.

    Returns:
        Nothing
    """
     
    if not os.path.exists(output_directory):
        os.makedirs(output_directory)

    for f_path, r_path, file_name in overlay_images_info:
        # Reload background for each combination to avoid overlaying on top of previous images
        background = cv2.imread(background_path)
        mask=None
        for overlay_path, (x, y, w, h) in zip(
            [f_path, r_path] if r_path else [f_path], positions_and_sizes
        ):
            imgShift = 0

            # Get flattened tombstone photo
            overlay = cv2.imread(overlay_path)
            
            # If the photo exists combine to create a texture
            if overlay_path:
                # If the current photo is a rear photo of the tombstone, set the proportions to equal that of the front photo of the same tombstone
                if overlay_path == r_path:
                    fpath_overlay = cv2.imread(f_path)

                    overlay = cv2.resize(
                        overlay, (fpath_overlay.shape[1], fpath_overlay.shape[0])
                    )

                # Resize the tombstone while keeping the dimensions proportional and within given bounds (height, width)
                overlay=cv2.copyMakeBorder(overlay,100,100,100,100,cv2.BORDER_CONSTANT,value=[0,0,0])
                overlay = resize_with_bounds(overlay, (600, resolution))
                black_mask_value = 10
                if(not(is_shadowed_image(overlay))):
                    
                    #test
                    overlay=np.clip(overlay, 0, 255).astype(np.float32)
                    overlay[overlay>black_mask_value]=overlay[overlay>black_mask_value]+((255-(overlay[overlay>black_mask_value]))/5)
                    overlay=np.clip(overlay, 0, 255).astype(np.uint8)
                    contrast_overlay = auto_balance_brightness_with_boost(overlay,max_gamma=.10)
                    overlay=np.clip(overlay, 0, 255).astype(np.float32)
                    #overlay[overlay>black_mask_value]=overlay[overlay>black_mask_value]+((255-(overlay[overlay>black_mask_value]))/4)
                    overlay=np.clip(overlay, 0, 255).astype(np.uint8)
                    overlay=contrast_overlay
                    #overlay=cv2.addWeighted(overlay,0.3,contrast_overlay,0.7,1)
                    overlay=cv2.cvtColor(overlay,cv2.COLOR_BGR2GRAY) 
                    overlay=cv2.cvtColor(overlay,cv2.COLOR_GRAY2BGR)
                    
                   
                else:
                    #test
                    overlay=np.clip(overlay, 0, 255).astype(np.float32)
                    overlay[overlay>black_mask_value]=overlay[overlay>black_mask_value]+((255-(overlay[overlay>black_mask_value]))/2) 
                    overlay=np.clip(overlay, 0, 255).astype(np.uint8)
                    overlay = auto_balance_brightness_with_boost(overlay,max_gamma=19.0)
                    overlay=np.clip(overlay, 0, 255).astype(np.float32)
                    overlay[overlay>black_mask_value]=overlay[overlay>black_mask_value]+((255-(overlay[overlay>black_mask_value]))/2)
                    overlay=np.clip(overlay, 0, 255).astype(np.uint8)
                    overlay=cv2.cvtColor(overlay,cv2.COLOR_BGR2GRAY)
                    
                    
                    
                    overlay=cv2.cvtColor(overlay,cv2.COLOR_GRAY2BGR)
                    overlay[overlay>255]=255
                    overlay[overlay<0]=255
                    
                #Match the background texture to the tombstone photo
                rounded_overlay=overlay.copy()
                rounded_overlay-cv2.GaussianBlur(rounded_overlay,(75,75),5,5)
                rounded_overlay=rounded_overlay[overlay>black_mask_value]
                rounded_overlay=(np.ceil(rounded_overlay/20.0)*20).astype(np.uint8)
                sorted_pixels = np.sort(rounded_overlay, axis=None)
                background=np.clip(background-((np.average(background)-np.median(sorted_pixels))/2),0,255).astype(np.uint8)
                
               
                # Defines the center placement of where the tombstone photo goes on the texture
                imgShift = abs(int(background.shape[1] / 4 - overlay.shape[1] / 2))

                # Perform the overlay

                # Defines the location the where the photo will go on the texture the [top left corner, top right corner, bottom left corner]
                imgPos = [
                    imgShift,
                    (imgShift + overlay.shape[1]),
                    (imgShift + overlay.shape[0]),
                ]

                #Area that bounds of the tombstoe image on the background texture
                

                # Places the front photo on the left side of the texture
                if overlay_path == f_path:
                    # Overlays the photo while ignoring values in the black mask
                    maskedBackground = background[
                        (imgPos[0]) : (imgPos[2]), (imgPos[0]) : (imgPos[1])
                    ]
                   
                    blended_texture=texture_blend(overlay,maskedBackground)
                    blended_texture=np.clip(blended_texture, 0, 255).astype(np.uint8)
                    background[(imgPos[0]) : (imgPos[2]), (imgPos[0]) : (imgPos[1])]=blended_texture

                # Places the rear photo on the right side of the texture
                else:
                    # Overlays the photo while ignoring values in the black mask
                    maskedBackground = background[
                        (imgPos[0]) : (imgPos[2]), (-imgPos[1]) : (-imgPos[0])
                    ]

                   

                    blended_texture=texture_blend(overlay,maskedBackground)
                    blended_texture=np.clip(blended_texture, 0, 255).astype(np.uint8)
                    background[(imgPos[0]) : (imgPos[2]), (-imgPos[1]) : (-imgPos[0])] = blended_texture


                print(f"Combining: {os.path.basename(overlay_path)} onto background")
        background=np.clip(background, 0, 255).astype(np.float32)
        background-=30
        background=np.clip(background, 0, 255).astype(np.uint8)
       
        # Save the combined image with the new naming convention, ensuring no extra dashes
        output_path = os.path.join(output_directory, f"{file_name}.png")
        cv2.imwrite(output_path, background)
        print(f"Saved combined image as {output_path}")


# Usage
directory_with_images = sys.argv[1]
output_directory = "FinalTextures"
background_image_path = "plainHeadstone.png"
overlay_images_info = find_overlay_images(directory_with_images)
positions_and_sizes = [
    (0, 0, 1005, 1590),
    (1005, 0, 1043, 1590),
]  # First for F, second for R if present
# Defines how wide the tombstone photo resolution can be in the 1024px x 1024px texture
maxPhotoWidth = 500
combine_images(
    background_image_path,
    overlay_images_info,
    output_directory,
    positions_and_sizes,
    resolution=maxPhotoWidth,
)
