#include <iostream>
#include <fstream>
#include <opencv2/opencv.hpp>
#include <numeric>
#include <ctime>
#include <errno.h>
#include <vector>
//#include "..\..\KinectScan\KinectLib\KinectLib.h"

using namespace std;
using namespace cv;

int main(int argc, char** argv)
{
	String filename = "Gatorade-Small.jpg";
	String filenameFiltered = "Gatorade-Small whiteFiltered.jpg";
	Mat img = imread(filename);
	//imwrite("test.jpg", img);
	//Mat whiteFiltered = KinectLib::GreenScreenFilter(img);
	


	Mat hsv, mask, imgFilt;
	cvtColor(img, hsv, COLOR_BGR2HSV);

	//// create a mask for green
	//inRange(hsv, Scalar(65, 60, 60), Scalar(80, 255, 255), mask);

	// create a mask for white
	inRange(hsv, Scalar(0, 0, 120), Scalar(255, 38, 255), mask);
	bitwise_or(img, img * 0 + 255, imgFilt, mask);
	//imshow("whiteFilterMask", mask);
	waitKey(0);
	imgFilt = img + (imgFilt);

	imwrite(filenameFiltered, imgFilt);







	//int MAX_KERNEL_LENGTH = 5;
	//int DELAY_BLUR = 100;

	//Mat img, imgBlur, hsv, mask, imgFilt, mask2;
	//img = imread("ColorCropImg1.jpg");
	////cv::imwrite("img.jpg", img);
	//
	////imshow("img", img);
	////waitKey(0);

	//// apply blur
	//for (int i = 1; i < MAX_KERNEL_LENGTH; i = i + 2)
	//{
	//	blur(img, imgBlur, Size(i, i), Point(-1, -1));
	//}
	///*imshow("imgBlur", imgBlur);
	//waitKey(0);*/
	//
	//// convert to HSV
	//cvtColor(img, hsv, COLOR_BGR2HSV);
	//// create a mask for green
	//inRange(hsv, Scalar(65, 60, 60), Scalar(80, 255, 255), mask);
	//imshow("mask", mask);
	//waitKey(0);
	//
	//imwrite("mask.jpg", mask);
	//mask = imread("mask.jpg");
	//imshow("mask", mask);
	//waitKey(0);



	////inRange(hsv, Scalar(72, 150, 76), Scalar(80, 255, 255), mask);
	//// remove mask from original img
	//bitwise_or(img, mask, imgFilt);

	////bitwise_and(img, 255-mask, imgFilt);
	////cvtColor(mask, mask2, COLOR_BGR2HSV);
	////cv::subtract(img, mask, imgFilt);
	//imwrite("imgFilt.jpg", imgFilt);
	//
	//imshow("imgFilt", imgFilt);
	//waitKey(0);

}