#pragma once

#include <k4a/k4a.h>
#include <json/json.h>
#include <opencv2/opencv.hpp>

using namespace std;
using namespace cv;

namespace KinectLib 
{
	void saveColorImg(k4a_image_t color_image, Json::Value AppConfig, int camNum);
	Mat getColorImg(k4a_image_t color_image);
	void saveDepthImg(k4a_image_t transformed_depth_image, Json::Value AppConfig, int camNum);
	Mat getDepthImg(k4a_image_t transformed_depth_image);
	Mat generateAndGetCroppedImg(Mat fullimage, k4a_image_t transformed_depth_image, Json::Value AppConfig, bool debugShow, bool isTopCam);
	Mat CropAndGetImg(Mat fullimage, Mat dept2colorhMat, std::vector<uint16_t> data, Json::Value AppConfig, bool debugShow);
	Mat CropAndGetTopCamImg(Mat fullimage, Mat dept2colorhMat, std::vector<uint16_t> data, Json::Value AppConfig, bool debugShow);
	Mat GreenScreenFilter(Mat img);
}