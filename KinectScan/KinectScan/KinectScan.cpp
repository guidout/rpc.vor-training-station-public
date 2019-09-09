#include <json/json.h>
#include <iostream>
#include <fstream>
#include <k4a/k4a.h>
#include <opencv2/opencv.hpp>
#include <numeric>
#include <ctime>
#include <errno.h>
//namespace KinectLib {
#include "..\..\KinectScan\KinectLib\KinectLib.h"
//}


#pragma warning(disable : 4996)

#define AppConfigFile "KinectScanConfig.json"
#define doNotCheckTopCamera true

using namespace std;
using namespace cv;
//using namespace KinectLib;

void UpdateConfigFile(Json::Value jsonContent, string fieldName, string fieldValue) {
	Json::StyledStreamWriter writer;
	Json::Value val = fieldValue;
	jsonContent[fieldName].swap(val);
	std::ofstream ifs(AppConfigFile);
	writer.write(ifs, jsonContent);
	ifs.close();
	cout << fieldValue << "\n";
}

int main(int argc, char** argv)
{
	bool debug = true;
	bool debugShow = false;
	// Read configuration file
	std::ifstream ifs(AppConfigFile);
	//for (std::string line; std::getline(ifs, line); )
	//{	
	//	std::cout << line << std::endl;
	//}
	Json::Reader reader;
	Json::Value AppConfig;
	Json::StyledStreamWriter writer; //to write changes back to file
	reader.parse(ifs, AppConfig);
	ifs.close();
	string test = AppConfig.get("ColorImg" + to_string(0) + "_name", "").asString();
	// Clear exit error
	UpdateConfigFile(AppConfig, "exitError", "");
	//////////////////////////
	
	k4a_device_t device[] = {NULL, NULL, NULL};
	uint32_t device_count = k4a_device_get_installed_count();
	k4a_device_configuration_t config = K4A_DEVICE_CONFIG_INIT_DISABLE_ALL;
	config.color_format = K4A_IMAGE_FORMAT_COLOR_BGRA32;
	config.color_resolution = K4A_COLOR_RESOLUTION_1080P;
	config.depth_mode = K4A_DEPTH_MODE_NFOV_UNBINNED;
	config.camera_fps = K4A_FRAMES_PER_SECOND_15;
	config.synchronized_images_only = true; // ensures that depth and color images are both available in the capture
	k4a_calibration_t nullCal;
	k4a_calibration_t calibration;
	k4a_capture_t capture;// = NULL;
	k4a_transformation_t transformation[] = { NULL, NULL, NULL };
	k4a_image_t color_image;
	k4a_image_t depth_image;
	k4a_image_t transformed_depth_image;// = NULL;
	k4a_image_format_t imgFormat;
	const int32_t IMU_Timeout = 100;
	k4a_imu_sample_t IMUsample;
	float CamXacc = -10000; // initial values of accelerations are saturated. IMU needs some time to start filling the register (I guess)
	uint8_t TopCamIdx = NULL;
	bool isTopCam;

	// Images
	Mat colorImg[3];
	Mat depthTemplate(Size(1920, 1080), CV_16UC1);
	Mat depthImg[3];// = { depthTemplate, depthTemplate, depthTemplate };
	Mat croppedImg[3];
	Mat depthTest;

	if (device_count == 0)
	{
		std::printf("No K4A devices found\n");
		UpdateConfigFile(AppConfig, "exitError", "No Kinects found");

		return 0;
	}

	///////////////////////////
	// OPEN ALL DEVICES
	for (uint32_t camNum = 0; camNum < device_count; camNum++)
	{
		if (K4A_RESULT_SUCCEEDED != k4a_device_open(camNum, &device[camNum]))
		{
			std::printf("Failed to open device\n");
			UpdateConfigFile(AppConfig, "exitError", "Failed to open device");
			return 0;
		}
		// hack to get win device name
		/*try {
			k4a_device_open(camNum, &device[camNum]);
			perror("");
		}
		catch (...) {

		}*/

		if (K4A_RESULT_SUCCEEDED != k4a_device_start_cameras(device[camNum], &config))
		{
			std::printf("Failed to start device\n");
			UpdateConfigFile(AppConfig, "exitError", "Failed to start device");
			return 0;
		}
		//////////////////////////////////////////////////////////////////////////////////////


		//// Get IMU sample to find top camera
		//if ( K4A_RESULT_SUCCEEDED != k4a_device_start_imu(device[camNum]) )
		//{
		//	std::printf("Failed to start IMU\n");
		//	UpdateConfigFile(AppConfig, "exitError", "Failed to start IMU");
		//	return 0;
		//}
		//while (abs(CamXacc) > 1000) {
		//	k4a_device_get_imu_sample(device[camNum], &IMUsample, IMU_Timeout);
		//	CamXacc = IMUsample.acc_sample.xyz.x;
		//	std::printf("X acc = ");
		//	std::printf("%6.4lf;\n", CamXacc);
		//	/*CamXacc = IMUsample.acc_sample.xyz.y;
		//	std::printf("Y acc = ");
		//	std::printf("%6.4lf;", CamXacc);
		//	CamXacc = IMUsample.acc_sample.xyz.z;
		//	std::printf("Z acc = ");
		//	std::printf("%6.4lf;", CamXacc);*/
		//	//cin.get();
		//}
		//if (CamXacc > 5) {
		//	TopCamIdx = camNum;
		//}
		////k4a_device_stop_imu(device[camNum]);
		//k4a_device_close(device[camNum]);
		//if (K4A_RESULT_SUCCEEDED != k4a_device_open(camNum, &device[camNum]))
		//{
		//	std::printf("Failed to re-open device\n");
		//	UpdateConfigFile(AppConfig, "exitError", "Failed to open device");
		//	return 0;
		//}
		//if (K4A_RESULT_SUCCEEDED != k4a_device_start_cameras(device[camNum], &config))
		//{
		//	std::printf("Failed to re-start device\n");
		//	UpdateConfigFile(AppConfig, "exitError", "Failed to re-start device");
		//	return 0;
		//}


		////////////////////////////////////////////////////////////////////////
		if (K4A_RESULT_SUCCEEDED !=
			k4a_device_get_calibration(device[camNum], config.depth_mode, config.color_resolution, &calibration))
		{
			std::printf("Failed to get calibration\n");
			UpdateConfigFile(AppConfig, "exitError", "Failed to get calibration");
			return 0;
		}
		transformation[camNum] = k4a_transformation_create(&calibration);

	}
	if (TopCamIdx == NULL & !doNotCheckTopCamera) {
		std::printf("Top Camera not found\n");
		UpdateConfigFile(AppConfig, "exitError", "Top Camera not found");
		return 0;
	}

	bool keepAlive = true;
	while (keepAlive) {
		time_t tstart, tend;
		tstart = time(0);
		// Get the pictures
		int returnCode = 1;
		const int32_t TIMEOUT_IN_MS = 1000;
		
		// Read configuration file again
		std::ifstream ifs(AppConfigFile);
		//Json::StyledStreamWriter writer; //to write changes back to file
		reader.parse(ifs, AppConfig);
		ifs.close();
		keepAlive = AppConfig.get("keepAlive", false).asBool();
		bool takePictures = AppConfig.get("takePictures", false).asBool();

		if (takePictures) 
		{
			for (int camNum = 0; camNum < device_count; camNum++)
			{
				// Get a depth frame
				switch (k4a_device_get_capture(device[camNum], &capture, TIMEOUT_IN_MS))
				{
				case K4A_WAIT_RESULT_SUCCEEDED:
					break;
				case K4A_WAIT_RESULT_TIMEOUT:
					std::printf("Timed out waiting for a capture\n");
					UpdateConfigFile(AppConfig, "exitError", "Timed out waiting for a capture");
					continue;
					break;
				case K4A_WAIT_RESULT_FAILED:
					std::printf("Failed to read a capture\n");
					UpdateConfigFile(AppConfig, "exitError", "Failed to read a capture");
					goto Exit;
				}
				std::printf("Capture");

				color_image = k4a_capture_get_color_image(capture);
				depth_image = k4a_capture_get_depth_image(capture);
				// Probe for a color image
				int imgWidth = k4a_image_get_width_pixels(color_image);
				int imgHeight = k4a_image_get_height_pixels(color_image);
				imgFormat = k4a_image_get_format(color_image);

				std::printf(" | Color res:%4dx%4d stride:%5d ",
					k4a_image_get_height_pixels(color_image),
					k4a_image_get_width_pixels(color_image),
					k4a_image_get_stride_bytes(color_image));

				std::printf(" | Depth16 res:%4dx%4d stride:%5d\n",
					k4a_image_get_height_pixels(depth_image),
					k4a_image_get_width_pixels(depth_image),
					k4a_image_get_stride_bytes(depth_image));

				// get and save depth image mapped to color image
				int color_image_width_pixels = k4a_image_get_width_pixels(color_image);
				int color_image_height_pixels = k4a_image_get_height_pixels(color_image);
				
				if (K4A_RESULT_SUCCEEDED != k4a_image_create(K4A_IMAGE_FORMAT_DEPTH16,
					color_image_width_pixels,
					color_image_height_pixels,
					color_image_width_pixels * (int)sizeof(uint16_t),
					&transformed_depth_image))
				{
					std::printf("Failed to create transformed depth image\n");
					UpdateConfigFile(AppConfig, "exitError", "Failed to create transformed depth image");
					return 0;
				}
				if (K4A_RESULT_SUCCEEDED != k4a_transformation_depth_image_to_color_camera(transformation[camNum], depth_image, transformed_depth_image)) {
					std::printf("Depth transformation failed");
					UpdateConfigFile(AppConfig, "exitError", "Depth transformation failed");
					return 0;
				}
				
				// GET PICTURES
				if (debug) {
					tend = time(0);
					cout << "It took " << difftime(tend, tstart) << " second(s) to prepare." << endl;
				}
				tstart = time(0);
				
				colorImg[camNum] = KinectLib::getColorImg(color_image);
				
				if (debug) {
					tend = time(0);
					cout << "It took " << difftime(tend, tstart) << " second(s) to save ColorImg." << endl;
				}
				tstart = time(0);

				depthImg[camNum] = KinectLib::getDepthImg(transformed_depth_image);
				
				//Mat depthTemp = KinectLib::getDepthImg(transformed_depth_image);
				//memcpy(depthTemp.data, depthImg[camNum].data, (1920*1080) * sizeof(uint16_t));
				
				if (debug) {
					tend = time(0);
					cout << "It took " << difftime(tend, tstart) << " second(s) to save DepthImg." << endl;
				}
				tstart = time(0);
				if (camNum == 0) { isTopCam = true;	}
				else { isTopCam = false; }

				croppedImg[camNum] = KinectLib::generateAndGetCroppedImg(colorImg[camNum], transformed_depth_image, AppConfig, debugShow, isTopCam);

				if (debug) {
					tend = time(0);
					cout << "It took " << difftime(tend, tstart) << " second(s) to crop and save ColorCropImg." << endl;
				}
				tstart = time(0);


				//cv::FileStorage file2("DepthImg.txt", cv::FileStorage::WRITE);
				//file2 << "dept2colorhMat" << dept2colorhMat;
				
				
				//// release all
				//k4a_image_release(color_image);
				//k4a_image_release(depth_image);
				//k4a_image_release(transformed_depth_image);
				//
				//k4a_capture_release(capture);
				//k4a_transformation_destroy(transformation[camNum]);
			}
			// All pictures are taken and postprocessed
			// Write img files now
			// TOP CAMERA
			cv::imwrite("ColorImg-Top.jpg", colorImg[0]);
			//imwrite("DepthImg-Top.png", depthTest2);
			//depthImg[0].convertTo(depthImg[0], CV_16UC1);
			imwrite("DepthImg-Top.png", depthImg[0] );
			cv::imwrite("ColorCropImg-Top.jpg", croppedImg[0]);
			// this "if" is just for debugging
			if (device_count == 3) 
			{
				int SmallCam = 1;
				int LargeCam = 2;
				if (croppedImg[1].cols > croppedImg[2].cols) 
				{
					LargeCam = 1;
					SmallCam = 2;
				}
				cv::imwrite("ColorImg-Small.jpg", colorImg[SmallCam]);
				cv::imwrite("DepthImg-Small.png", depthImg[SmallCam]);
				cv::imwrite("ColorCropImg-Small.jpg", croppedImg[SmallCam]);
				cv::imwrite("ColorImg-Large.jpg", colorImg[LargeCam]);
				cv::imwrite("DepthImg-Large.png", depthImg[LargeCam]);
				cv::imwrite("ColorCropImg-Large.jpg", croppedImg[LargeCam]);
			}
		}
	}
	// Close all devices
	for (uint32_t camNum = 0; camNum < device_count; camNum++)
	{
		k4a_device_close(device[camNum]);
	}
Exit:
	if (device[0] != NULL)
	{
		k4a_device_close(device[0]);
	}
	return 0;
}

