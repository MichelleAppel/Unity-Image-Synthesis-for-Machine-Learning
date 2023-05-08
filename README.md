# Simulation Synthesis for Machine Learning

Generate a machine learning synthesis dataset from a Unity simulation. This dataset includes multiple modalities: RGB, object segmentation, normals, depth, and outlines.

This project is based on the Unity Technologies ML Image Synthesis repository: https://bitbucket.org/Unity-Technologies/ml-imagesynthesis/src/master/.

![Example dataset](https://user-images.githubusercontent.com/17069785/152363935-74475b8c-f106-40cb-a66b-53458b2b8b8e.png)

## Features

- Capture RGB images from the Unity simulation
- Generate object segmentation masks for individual object instances
- Compute surface normals and depth maps
- Create outline images using edge detection techniques
- Easily integrate with machine learning frameworks

## Getting Started

1. Clone the repository and open the project in Unity.
2. Set up the desired simulation scene and attach the camera scripts provided to your Unity cameras.
3. Configure the camera settings and choose the desired output modalities.
4. Run the simulation and start capturing the dataset.
5. Export the dataset for use in machine learning frameworks such as TensorFlow or PyTorch.

## Customizing the Dataset

You can easily customize the dataset by adjusting the camera settings or creating new camera scripts to capture additional modalities. Modify the provided scripts or create new ones to suit your specific needs and requirements.
