#include <json/json.h>
#include <iostream>
#include <fstream>
#include <k4a/k4a.h>
#include <opencv2/opencv.hpp>
#include <numeric>
#include <ctime>
#include <errno.h>
#include "KinectLib.h"

#pragma warning(disable : 4996)

#define AppConfigFile "KinectScanConfig.json"

using namespace std;
using namespace cv;


int main(int argc, char** argv)
{
	// load AppConfig
	//std::ifstream ifs(AppConfigFile);
	//Json::Reader reader;
	//Json::Value AppConfig;
	//Json::StyledStreamWriter writer; //to write changes back to file
	//reader.parse(ifs, AppConfig);
	//ifs.close();


	int numcam = 0;
	std::ifstream ifs(AppConfigFile);
	
	Json::Reader reader;
	Json::Value AppConfig;
	reader.parse(ifs, AppConfig);
	ifs.close();
	//cout << AppConfig.get("ColorImg" + to_string(numcam) + "_name", "").asString();
	//string test = AppConfig.get("ColorImg" + to_string(0) + "_name", "").asString();
	// read file
	bool debug = true;

	cv::Mat depthImage;
	//cout << SamplePicBasePath + "DepthImg2.jpg";
	double min = 0;
	double max = 5000;
	cv::minMaxIdx(depthImage, &min, &max);
	depthImage = cv::imread("C:\\Users\\gritelli\\Desktop\\8pack 2L bottle V2\\DepthImg1.png", cv::IMREAD_LOAD_GDAL | cv::IMREAD_ANYDEPTH);
	Mat fullImage = imread("C:\\Users\\gritelli\\Desktop\\8pack 2L bottle V2\\ColorImg1.jpg");
	//imshow("", depthImage);
	//waitKey(0);
	
	std::vector<uint16_t> data;
	uint16_t* depth2color_buffer = (uint16_t*)(void*)depthImage.data;
	// parse depth_data into vector
	for (int n = 0; n < depthImage.cols * depthImage.rows; n++) {
		data.push_back(depth2color_buffer[n]);
		//data.push_back(depthImage.data[n]);
	}
	
	//cv::FileStorage file2(SamplePicBasePath + "DepthImg.txt", cv::FileStorage::WRITE);
	//file2 << "depthImage" << depthImage;
	// run crop stuff
	Mat croppedImg = KinectLib::CropAndGetImg(fullImage, depthImage, data, AppConfig, false);
	imshow("croppedImg", croppedImg);
	waitKey(0);
}
