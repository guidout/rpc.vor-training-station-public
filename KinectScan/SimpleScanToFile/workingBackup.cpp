//#include <math.h>
//#include <string>
//#include <iostream>
//#include <fstream>
//#include <sstream>
//#include <stdio.h>
//#include <k4a/k4a.h>
//#include <opencv2/opencv.hpp>
////#include <opencv2/opencv_modules.hpp>
////#include <opencv2/core/mat.hpp>
//
//int main(int argc, char** argv)
//{
//	int returnCode = 1;
//	k4a_device_t device = NULL;
//	const int32_t TIMEOUT_IN_MS = 1000;
//	int captureFrameCount;
//	k4a_capture_t capture = NULL;
//	int imgWidthTemp = 1280;
//	int imgHeightTemp = 720;
//	int imgStrideTemp = 0;
//
//	//captureFrameCount = atoi(argv[1]);
//	captureFrameCount = 2;
//	printf("Capturing %d frames\n", captureFrameCount);
//
//	uint32_t device_count = k4a_device_get_installed_count();
//
//	if (device_count == 0)
//	{
//		printf("No K4A devices found\n");
//		return 0;
//	}
//
//	if (K4A_RESULT_SUCCEEDED != k4a_device_open(K4A_DEVICE_DEFAULT, &device))
//	{
//		printf("Failed to open device\n");
//		return 0;
//	}
//
//	k4a_device_configuration_t config = K4A_DEVICE_CONFIG_INIT_DISABLE_ALL;
//	config.color_format = K4A_IMAGE_FORMAT_COLOR_BGRA32;
//	config.color_resolution = K4A_COLOR_RESOLUTION_720P;
//	config.depth_mode = K4A_DEPTH_MODE_WFOV_2X2BINNED;
//	config.camera_fps = K4A_FRAMES_PER_SECOND_30;
//
//	if (K4A_RESULT_SUCCEEDED != k4a_device_start_cameras(device, &config))
//	{
//		printf("Failed to start device\n");
//		return 0;
//	}
//
//
//
//	while (captureFrameCount-- > 0)
//	{
//		// Get a depth frame
//		switch (k4a_device_get_capture(device, &capture, TIMEOUT_IN_MS))
//		{
//		case K4A_WAIT_RESULT_SUCCEEDED:
//			break;
//		case K4A_WAIT_RESULT_TIMEOUT:
//			printf("Timed out waiting for a capture\n");
//			continue;
//			break;
//		case K4A_WAIT_RESULT_FAILED:
//			printf("Failed to read a capture\n");
//			goto Exit;
//			return 0;
//		}
//
//		printf("Capture");
//
//		// Probe for a color image
//		k4a_image_t image;
//		image = k4a_capture_get_color_image(capture);
//
//		if (image)
//		{
//			int imgWidth = k4a_image_get_width_pixels(image);
//			int imgHeight = k4a_image_get_height_pixels(image);
//			k4a_image_format_t imgFormat = k4a_image_get_format(image);
//
//			uint8_t* image_data = (uint8_t*)(void*)k4a_image_get_buffer(image);
//			cv::Mat color_frame = cv::Mat(k4a_image_get_height_pixels(image), k4a_image_get_width_pixels(image), CV_8UC4, image_data, cv::Mat::AUTO_STEP);
//			cv::imwrite("testImg.jpg", color_frame);
//			cv::FileStorage file("testImg.txt", cv::FileStorage::WRITE);
//			file << "color_frame" << color_frame;
//			k4a_image_release(image);
//			//cv::imshow("color", color_frame);
//			// Declare what you need
//			//cv::FileStorage file("testCV.jpg", cv::FileStorage::WRITE);
//			//cv::Mat someMatrixOfAnyType;
//			// Write to file!
//			//file << "matName" << someMatrixOfAnyType;
//			printf(" | Color res:%4dx%4d stride:%5d ",
//				k4a_image_get_height_pixels(image),
//				k4a_image_get_width_pixels(image),
//				k4a_image_get_stride_bytes(image));
//		}
//		else
//		{
//			printf(" | Color None                       ");
//		}
//
//		// probe for a IR16 image
//		image = k4a_capture_get_ir_image(capture);
//		uint16_t* image_data = (uint16_t*)(void*)k4a_capture_get_ir_image(capture);
//		if (image != NULL)
//		{
//			printf(" | Ir16 res:%4dx%4d stride:%5d ",
//				k4a_image_get_height_pixels(image),
//				k4a_image_get_width_pixels(image),
//				k4a_image_get_stride_bytes(image));
//			k4a_image_release(image);
//			//cv::Mat depthMat = cv::Mat (k4a_image_get_height_pixels(image), k4a_image_get_width_pixels(image), CV_16U, image_data, cv::Mat::AUTO_STEP);
//			//cv::imwrite("testImgIR.jpg", depthMat);
//		}
//		else
//		{
//			printf(" | Ir16 None                       ");
//		}
//
//		// Probe for a depth16 image
//		image = k4a_capture_get_depth_image(capture);
//		uint16_t* buffer = (uint16_t*)(void*)k4a_image_get_buffer(image);
//		if (image != NULL)
//		{
//			printf(" | Depth16 res:%4dx%4d stride:%5d\n",
//				k4a_image_get_height_pixels(image),
//				k4a_image_get_width_pixels(image),
//				k4a_image_get_stride_bytes(image));
//			k4a_image_release(image);
//
//			cv::Mat depthMat = cv::Mat(k4a_image_get_height_pixels(image), k4a_image_get_width_pixels(image), CV_16U, buffer, cv::Mat::AUTO_STEP);
//			cv::imwrite("testImgDepth.jpg", depthMat);
//			cv::FileStorage file("testImgDepth.txt", cv::FileStorage::WRITE);
//			file << "depthMat" << depthMat;
//		}
//		else
//		{
//			printf(" | Depth16 None\n");
//		}
//
//		// release capture
//		k4a_capture_release(capture);
//	}
//
//	returnCode = 0;
//Exit:
//	if (device != NULL)
//	{
//		k4a_device_close(device);
//	}
//
//	return returnCode;
//}
