#include <math.h>
#include <string>
#include <iostream>
#include <fstream>
#include <sstream>
#include <stdio.h>
#include <k4a/k4a.h>
#include <opencv2/opencv.hpp>
//#include <opencv2/opencv_modules.hpp>
//#include <opencv2/core/mat.hpp>

int main(int argc, char** argv)
{
	int returnCode = 1;
	k4a_device_t device = NULL;
	const int32_t TIMEOUT_IN_MS = 1000;
	k4a_capture_t capture = NULL;

	uint32_t device_count = k4a_device_get_installed_count();

	if (device_count == 0)
	{
		printf("No K4A devices found\n");
		return 0;
	}

	if (K4A_RESULT_SUCCEEDED != k4a_device_open(K4A_DEVICE_DEFAULT, &device))
	{
		printf("Failed to open device\n");
		return 0;
	}

	k4a_device_configuration_t config = K4A_DEVICE_CONFIG_INIT_DISABLE_ALL;
	config.color_format = K4A_IMAGE_FORMAT_COLOR_BGRA32;
	config.color_resolution = K4A_COLOR_RESOLUTION_720P;
	config.depth_mode = K4A_DEPTH_MODE_NFOV_UNBINNED;
	config.camera_fps = K4A_FRAMES_PER_SECOND_30;
	config.synchronized_images_only = true; // ensures that depth and color images are both available in the capture

	k4a_calibration_t calibration;
	if (K4A_RESULT_SUCCEEDED !=
		k4a_device_get_calibration(device, config.depth_mode, config.color_resolution, &calibration))
	{
		printf("Failed to get calibration\n");
		return 0;
	}
	k4a_transformation_t transformation = k4a_transformation_create(&calibration);

	if (K4A_RESULT_SUCCEEDED != k4a_device_start_cameras(device, &config))
	{
		printf("Failed to start device\n");
		return 0;
	}

	bool GotColor = false;
	bool GotDepth = false;

	while (!GotColor || !GotDepth)
	{
		// Get a depth frame
		switch (k4a_device_get_capture(device, &capture, TIMEOUT_IN_MS))
		{
		case K4A_WAIT_RESULT_SUCCEEDED:
			break;
		case K4A_WAIT_RESULT_TIMEOUT:
			printf("Timed out waiting for a capture\n");
			continue;
			break;
		case K4A_WAIT_RESULT_FAILED:
			printf("Failed to read a capture\n");
			goto Exit;
			return 0;
		}
		k4a_image_t image;
		printf("Capture");
		char ImgName[] = "Img1";

		k4a_image_t color_image = k4a_capture_get_color_image(capture);
		k4a_image_t depth_image = k4a_capture_get_depth_image(capture);
		// Probe for a color image
		if (color_image && depth_image)
		{
			int imgWidth = k4a_image_get_width_pixels(color_image);
			int imgHeight = k4a_image_get_height_pixels(color_image);
			k4a_image_format_t imgFormat = k4a_image_get_format(color_image);

			// get and save color image
			uint8_t* image_data = (uint8_t*)(void*)k4a_image_get_buffer(color_image);
			cv::Mat color_frame = cv::Mat(k4a_image_get_height_pixels(color_image), k4a_image_get_width_pixels(color_image), CV_8UC4, image_data, cv::Mat::AUTO_STEP);
			cv::imwrite("ColorImg.jpg", color_frame);
			//cv::FileStorage file("ColorImg.txt", cv::FileStorage::WRITE);
			//file << "color_frame" << color_frame;
			//file.release();
			k4a_image_release(color_image);
			printf(" | Color res:%4dx%4d stride:%5d ",
				k4a_image_get_height_pixels(color_image),
				k4a_image_get_width_pixels(color_image),
				k4a_image_get_stride_bytes(color_image));
			
			// get and save raw depth16 image
			uint16_t* buffer = (uint16_t*)(void*)k4a_image_get_buffer(depth_image);
		
			printf(" | Depth16 res:%4dx%4d stride:%5d\n",
				k4a_image_get_height_pixels(depth_image),
				k4a_image_get_width_pixels(depth_image),
				k4a_image_get_stride_bytes(depth_image));
			k4a_image_release(depth_image);

			//cv::Mat depthMat = cv::Mat(k4a_image_get_height_pixels(depth_image), k4a_image_get_width_pixels(depth_image), CV_16U, buffer, cv::Mat::AUTO_STEP);
			//cv::imwrite("DepthImg.jpg", depthMat);
			//cv::FileStorage file("DepthImg.txt", cv::FileStorage::WRITE);
			//file << "depthMat" << depthMat;

			// get and save depth image mapped to color image
			int color_image_width_pixels = k4a_image_get_width_pixels(color_image);
			int color_image_height_pixels = k4a_image_get_height_pixels(color_image);
			k4a_image_t transformed_depth_image = NULL;
			if (K4A_RESULT_SUCCEEDED != k4a_image_create(K4A_IMAGE_FORMAT_DEPTH16,
				color_image_width_pixels,
				color_image_height_pixels,
				color_image_width_pixels * (int)sizeof(uint16_t),
				&transformed_depth_image))
			{
				printf("Failed to create transformed depth image\n");
				return false;
			}
			if (K4A_RESULT_SUCCEEDED != k4a_transformation_depth_image_to_color_camera(transformation, depth_image, transformed_depth_image)) {
				printf("Depth transformation failed");
				return 0;
			}
			uint16_t* depth2color_buffer = (uint16_t*)(void*)k4a_image_get_buffer(transformed_depth_image);
			cv::Mat dept2colorhMat = cv::Mat(k4a_image_get_height_pixels(transformed_depth_image), k4a_image_get_width_pixels(transformed_depth_image), CV_16U, depth2color_buffer, cv::Mat::AUTO_STEP);
			cv::imwrite("DepthImg.jpg", dept2colorhMat);
			
			cv::FileStorage file2("DepthImg.txt", cv::FileStorage::WRITE);
			file2 << "dept2colorhMat" << dept2colorhMat;

			GotColor = true;
			GotDepth = true;
		}
		// release capture
		k4a_capture_release(capture);
	}

	returnCode = 0;
Exit:
	if (device != NULL)
	{
		k4a_device_close(device);
	}

	return returnCode;
}
