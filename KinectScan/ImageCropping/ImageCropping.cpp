#include <math.h>
#include <string>
#include <iostream>
#include <fstream>
#include <sstream>
#include <stdio.h>
#include <vector>
#include <algorithm>
#include <numeric>
#include <opencv2/opencv.hpp>
#include <opencv2/dnn.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/highgui.hpp>
//#include "common.hpp"
using namespace cv;

bool IsNotZero(int i) { return (i != 0); }

int main(int argc, char** argv)
{
	bool debug = true;

	cv::FileStorage imgFile("C:\\Users\\gritelli\\source\\repos\\KinectScan\\SimpleScanToFile\\DepthImg4.txt", cv::FileStorage::READ);
	cv::Mat dept2colorhMat;
	imgFile["dept2colorhMat"] >> dept2colorhMat;

	if (debug) {
		cv::imshow("Original depth image", dept2colorhMat);
		waitKey(0);
	}

	std::vector<uint16_t> data;

	//***********************************************
	// get the depth data
	uint16_t* depth_data = (uint16_t*)(void*)dept2colorhMat.data;

	//***********************************************
	// parse depth_data into vector
	for (int n = 0; n < dept2colorhMat.cols * dept2colorhMat.rows; n++) {
		data.push_back(depth_data[n]);
	}

	//***********************************************
	// saturate all the pixel with zero value
	for (int n = 0; n < data.size(); n++) {
		if (data[n] == 0) {
			data[n] = UINT16_MAX;
		}
	}

	//***********************************************
	// get the distance of the closest object
	uint16_t PixelNum = 200;
	std::vector<uint16_t> dataTemp = data;
	std::sort(dataTemp.begin(), dataTemp.end());
	uint16_t MinDistObj = std::accumulate(dataTemp.begin(), dataTemp.begin()+ PixelNum, 0)/ PixelNum;

	//***********************************************
	//set to zero every pixel at different distance of MinDistObj + -Delta
	uint16_t DeltaDistance = 100;

	std::vector<uint16_t> DepthImgWithinDistance = data;
	for (int n = 0; n < data.size(); n++) {
		if (data[n] < MinDistObj - DeltaDistance | data[n] > MinDistObj + DeltaDistance) {
			DepthImgWithinDistance[n] = 0;
		}
	}
	// show
	if (debug) {
		cv::Mat DepthImgClosestObj = Mat(dept2colorhMat.rows, dept2colorhMat.cols, CV_8UC1);
		//DepthImgClosestObj.data = *DepthImgWithinDistance;
		for (int n = 0; n < dept2colorhMat.rows * dept2colorhMat.cols; n++) {
			*(DepthImgClosestObj.data + n) = DepthImgWithinDistance[n];
		}
		//DepthImgClosestObj.data = DepthImgWithinDistance;
		cv::imshow("DepthImgClosestObj", DepthImgClosestObj);
		waitKey(0);
	}
	
	//***********************************************
	// detect edges
	double edgeTreshold_Vert = 0.2;
	double edgeTreshold_HorizTop = 0.4;
	double edgeTreshold_HorizBottom = 0.7;
	
	int count;
	std::vector<double> ObjRows;
	for (int n = 0; n < dept2colorhMat.rows; n++) {
		count = 0;
		//ObjRows.push_back(count_if(DepthImgWithinDistance.begin() + (dept2colorhMat.cols*n), DepthImgWithinDistance.begin() + (n + 1) * dept2colorhMat.cols, IsNotZero));
		for (int col = 0; col < dept2colorhMat.cols; col++) {
			if (DepthImgWithinDistance[dept2colorhMat.cols*n + col] != 0) {
				count++;
			}
		}
		ObjRows.push_back(count);
	}
	uint16_t maxObjRows = *std::max_element(ObjRows.begin(), ObjRows.end());
	std::vector<double> ObjRowsIdx = ObjRows;
	transform(ObjRowsIdx.begin(), ObjRowsIdx.end(), ObjRowsIdx.begin(), [maxObjRows](auto& c) {return c / maxObjRows; });
	bool isInRange_prev = false;
	bool isInRange_curr = false;
	int HorizEdge[] = { 0,0 };
	// start from the top to get first element within threshold
	for (int n = 0; n < ObjRowsIdx.size(); n++) {
		isInRange_curr = ObjRowsIdx[n] > edgeTreshold_HorizTop;
		if (!isInRange_prev & isInRange_curr) {
			HorizEdge[0] = n;
			break;
		}
		isInRange_prev = isInRange_curr;
	}
	// start from the bottom to get last element within threshold
	isInRange_prev = false;
	for (int n = ObjRowsIdx.size()-1; n > 0; n--) {
		isInRange_curr = ObjRowsIdx[n] > edgeTreshold_HorizBottom;
		if (!isInRange_prev & isInRange_curr) {
			HorizEdge[1] = n;
			break;
		}
		isInRange_prev = isInRange_curr;
	}

	std::vector<double> ObjCols;
	for (int n = 0; n < dept2colorhMat.cols; n++) {
		count = 0;
		for (int row = 0; row < dept2colorhMat.rows; row++) {
			if (DepthImgWithinDistance[n + row * dept2colorhMat.cols] != 0) {
				count++;
			}
		}
		ObjCols.push_back(count);
	}
	uint16_t maxObjCols = *std::max_element(ObjCols.begin(), ObjCols.end());
	std::vector<double> ObjColsIdx = ObjCols;
	transform(ObjColsIdx.begin(), ObjColsIdx.end(), ObjColsIdx.begin(), [maxObjCols](auto& c) {return c / maxObjCols; });
	int VertEdge[] = { 0,0 };
	// start from left to find first element within threshold
	isInRange_prev = false;
	for (int n = 0; n < ObjColsIdx.size(); n++) {
		isInRange_curr = ObjColsIdx[n] > edgeTreshold_Vert;
		if (!isInRange_prev & isInRange_curr) {
			VertEdge[0] = n;
			break;
		}
		isInRange_prev = isInRange_curr;
	}
	// start from right to find last element within threshold
	isInRange_prev = false;
	for (int n = ObjColsIdx.size()-1; n > 0 ; n--) {
		isInRange_curr = ObjColsIdx[n] > edgeTreshold_Vert;
		if (!isInRange_prev & isInRange_curr) {
			VertEdge[1] = n;
			break;
		}
		isInRange_prev = isInRange_curr;
	}

	Mat image = imread("C:\\Users\\gritelli\\source\\repos\\KinectScan\\SimpleScanToFile\\ColorImg4.jpg");
	Mat cropedImage =  image(Rect(VertEdge[0], HorizEdge[0], VertEdge[1] - VertEdge[0], HorizEdge[1] - HorizEdge[0]));
	imshow("cropped", cropedImage);
	waitKey(0);
	imwrite("C:\\Users\\gritelli\\source\\repos\\KinectScan\\SimpleScanToFile\\ColorImgCropped4.jpg", cropedImage);
	
	
	
	
	
	
	


	// Reassign the new data to picture and show it
	/*dept2colorhMat.data = (uchar*)depth_data;
	cv::imshow("test", dept2colorhMat);
	cv::waitKey(0);*/


}