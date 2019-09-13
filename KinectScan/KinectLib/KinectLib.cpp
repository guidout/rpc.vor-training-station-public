// KinectLib.cpp : Defines the functions for the static library.
//

#include <k4a/k4a.h>
#include <json/json.h>
#include <opencv2/opencv.hpp>
#include "KinectLib.h"
#include <numeric>

// TODO: This is an example of a library function
namespace KinectLib
{
	void saveColorImg(k4a_image_t color_image, Json::Value AppConfig, int camNum) {
		uint8_t* image_data = (uint8_t*)(void*)k4a_image_get_buffer(color_image);
		cv::Mat color_frame = cv::Mat(k4a_image_get_height_pixels(color_image), k4a_image_get_width_pixels(color_image), CV_8UC4, image_data, cv::Mat::AUTO_STEP);
		cv::imwrite(AppConfig.get("ColorImg" + to_string(camNum) + "_name", "").asString(), color_frame);
		color_frame.release();
	}
	Mat getColorImg(k4a_image_t color_image) {
		uint8_t* image_data = (uint8_t*)(void*)k4a_image_get_buffer(color_image);
		cv::Mat color_frame = cv::Mat(k4a_image_get_height_pixels(color_image), k4a_image_get_width_pixels(color_image), CV_8UC4, image_data, cv::Mat::AUTO_STEP);
		return color_frame;
	}
	void saveDepthImg(k4a_image_t transformed_depth_image, Json::Value AppConfig, int camNum) {
		uint16_t* depth2color_buffer = (uint16_t*)(void*)k4a_image_get_buffer(transformed_depth_image);
		cv::Mat dept2colorhMat = cv::Mat(k4a_image_get_height_pixels(transformed_depth_image), k4a_image_get_width_pixels(transformed_depth_image), CV_16U, depth2color_buffer, cv::Mat::AUTO_STEP);
		//if (true) {
		//	imshow("Depth camera picture", dept2colorhMat);
		//	waitKey(0);
		//}
		cv::imwrite(AppConfig.get("DepthImg" + to_string(camNum) + "_name", "").asString(), dept2colorhMat);
		dept2colorhMat.release();
	}
	Mat getDepthImg(k4a_image_t transformed_depth_image) {
		uint16_t* depth2color_buffer = (uint16_t*)(void*)k4a_image_get_buffer(transformed_depth_image);
		Mat depth2colorMat = Mat(k4a_image_get_height_pixels(transformed_depth_image), k4a_image_get_width_pixels(transformed_depth_image), CV_16U, depth2color_buffer, cv::Mat::AUTO_STEP);
		return depth2colorMat;
	}
	
	Mat generateAndGetCroppedImg(Mat fullimage, k4a_image_t transformed_depth_image, Json::Value AppConfig, bool debugShow, bool isTopCam) {
		uint16_t* depth2color_buffer = (uint16_t*)(void*)k4a_image_get_buffer(transformed_depth_image);
		cv::Mat dept2colorhMat = cv::Mat(k4a_image_get_height_pixels(transformed_depth_image), k4a_image_get_width_pixels(transformed_depth_image), CV_16U, depth2color_buffer, cv::Mat::AUTO_STEP);
		if (debugShow) {
			imshow("Depth camera picture", dept2colorhMat);
			waitKey(0);
		}
		std::vector<uint16_t> data;
		// parse depth_data into vector
		for (int n = 0; n < dept2colorhMat.cols * dept2colorhMat.rows; n++) {
			data.push_back(depth2color_buffer[n]);
		}

		if (isTopCam) {
			Mat croppedAndFiltImg = CropAndGetTopCamImg(fullimage, dept2colorhMat, data, AppConfig, debugShow);
			return croppedAndFiltImg;
		}
		else {
			Mat croppedAndFiltImg = CropAndGetImg(fullimage, dept2colorhMat, data, AppConfig, debugShow);
			return croppedAndFiltImg;
		}
		

		dept2colorhMat.release();
	}
	
	Mat CropAndGetImg(Mat fullimage, Mat dept2colorhMat, std::vector<uint16_t> data, Json::Value AppConfig, bool debugShow)
	{
		Mat emptyMat;
		uint16_t DeltaDistance = AppConfig.get("DepthDelta_mm", 75).asInt();
		uint16_t DeltaDistanceTopMultiplier = 3;
		//***********************************************
		// saturate all the pixel with zero value and outside of the margins
		//for (int n = 0; n < data.size(); n++) {
		//	if (data[n] == 0) {
		//		data[n] = UINT16_MAX;
		//	}
		//}
		int FrameMarginSides_Left = AppConfig.get("FrameMarginLRSides", 0).asInt();
		int FrameMarginSides_Right = dept2colorhMat.cols - AppConfig.get("FrameMarginLRSides", 0).asInt();
		cout << "LR Sides margin is: ";
		cout << AppConfig.get("FrameMarginLRSides", 0).asInt() << endl;
		int FrameMarginSides_Top = AppConfig.get("FrameMarginTBSides", 0).asInt();
		int FrameMarginSides_Bottom = dept2colorhMat.rows - AppConfig.get("FrameMarginTBSides", 0).asInt();
		cout << "TB Sides margin is: ";
		cout << AppConfig.get("FrameMarginTBSides", 0).asInt() << endl;

		for (int n = 0; n < dept2colorhMat.rows; n++) {
			for (int col = 0; col < dept2colorhMat.cols; col++) {
				// margins
				if (n < FrameMarginSides_Top | n > FrameMarginSides_Bottom
					| col <FrameMarginSides_Left | col > FrameMarginSides_Right) {
					data[n*dept2colorhMat.cols + col] = UINT16_MAX;
				}
				// zero valued pixels
				else if (data[dept2colorhMat.cols * n + col] == 0) {
					data[n * dept2colorhMat.cols + col] = UINT16_MAX;
				}
			}
		}
		if (debugShow) {
			cv::Mat DepthImgSaturated = cv::Mat(dept2colorhMat.rows, dept2colorhMat.cols, CV_16U);
			memcpy(DepthImgSaturated.data, data.data(), data.size() * sizeof(uint16_t));
			cv::imshow("DepthImgSaturated", DepthImgSaturated);
			waitKey(0);
		}

		//***********************************************
		// get the distance of the closest object
		uint16_t PixelNum = 200;
		std::vector<uint16_t> dataTemp = data;
		std::sort(dataTemp.begin(), dataTemp.end());
		uint16_t MinDistObj = std::accumulate(dataTemp.begin(), dataTemp.begin() + PixelNum, 0) / PixelNum;

		//***********************************************
		//set to zero every pixel at different distance of MinDistObj + -Delta
		
		std::vector<uint16_t> DepthImgWithinDistance = data;
		for (int n = 0; n < data.size(); n++) {
			if (data[n] < MinDistObj - DeltaDistance | data[n] > MinDistObj + DeltaDistance) {
				DepthImgWithinDistance[n] = 0;
			}
		}
		std::vector<uint16_t> DepthImgWithinDistanceTop = data;
		for (int n = 0; n < data.size(); n++) {
			if (data[n] < MinDistObj - (DeltaDistance*DeltaDistanceTopMultiplier) | data[n] > MinDistObj + (DeltaDistance*DeltaDistanceTopMultiplier) ) {
				DepthImgWithinDistanceTop[n] = 0;
			}
		}
		// show
		if (debugShow) {
			cv::Mat DepthImgClosestObj = Mat(dept2colorhMat.rows, dept2colorhMat.cols, CV_8UC1);
			//DepthImgClosestObj.data = *DepthImgWithinDistance;
			for (int n = 0; n < dept2colorhMat.rows * dept2colorhMat.cols; n++) {
				*(DepthImgClosestObj.data + n) = DepthImgWithinDistance[n];
			}
			//cv::Mat DepthImgClosestObj = cv::Mat(dept2colorhMat.rows, dept2colorhMat.cols, CV_16U);
			//memcpy(DepthImgClosestObj.data, DepthImgWithinDistance.data(), DepthImgWithinDistance.size() * sizeof(uint16_t));
			
			cv::imshow("DepthImgClosestObj", DepthImgClosestObj);
			waitKey(0);
		}

		//***********************************************
		// detect edges
		//double edgeTreshold_Vert = 0.2;
		//double edgeTreshold_HorizTop = 0.4;
		//double edgeTreshold_HorizBottom = 0.7;

		int count;
		std::vector<double> ObjRows;
		for (int n = 0; n < dept2colorhMat.rows; n++) {
			count = 0;
			//ObjRows.push_back(count_if(DepthImgWithinDistance.begin() + (dept2colorhMat.cols*n), DepthImgWithinDistance.begin() + (n + 1) * dept2colorhMat.cols, IsNotZero));
			for (int col = 0; col < dept2colorhMat.cols; col++) {
				if (DepthImgWithinDistance[dept2colorhMat.cols * n + col] != 0) {
					count++;
				}
			}
			ObjRows.push_back(count);
		}
		uint16_t maxObjRows = *std::max_element(ObjRows.begin(), ObjRows.end());
		std::vector<double> ObjRowsIdx = ObjRows;
		transform(ObjRowsIdx.begin(), ObjRowsIdx.end(), ObjRowsIdx.begin(), [maxObjRows](auto& c) {return c / maxObjRows; });

		// Calculate using deeper distance
		std::vector<double> ObjRowsTop;
		for (int n = 0; n < dept2colorhMat.rows; n++) {
			count = 0;
			//ObjRows.push_back(count_if(DepthImgWithinDistance.begin() + (dept2colorhMat.cols*n), DepthImgWithinDistance.begin() + (n + 1) * dept2colorhMat.cols, IsNotZero));
			for (int col = 0; col < dept2colorhMat.cols; col++) {
				if (DepthImgWithinDistanceTop[dept2colorhMat.cols * n + col] != 0) {
					count++;
				}
			}
			ObjRowsTop.push_back(count);
		}
		uint16_t maxObjRowsTop = *std::max_element(ObjRowsTop.begin(), ObjRowsTop.end());
		std::vector<double> ObjRowsIdxTop = ObjRowsTop;
		transform(ObjRowsIdxTop.begin(), ObjRowsIdxTop.end(), ObjRowsIdxTop.begin(), [maxObjRowsTop](auto& c) {return c / maxObjRowsTop; });
		


		bool isInRange_prev = false;
		bool isInRange_curr = false;
		int HorizEdge[] = { 0,0 };
		// start from the top to get first element within threshold
		for (int n = 0; n < ObjRowsIdxTop.size(); n++) {
			isInRange_curr = ObjRowsIdxTop[n] > AppConfig.get("ImgCrop_edgeTreshold_HorizTop", 0.05).asDouble(); // edgeTreshold_HorizTop;
			if (!isInRange_prev & isInRange_curr) {
				HorizEdge[0] = n;
				break;
			}
			isInRange_prev = isInRange_curr;
		}
		// start from the bottom to get last element within threshold
		isInRange_prev = false;
		for (int n = ObjRowsIdx.size() - 1; n > 0; n--) {
			isInRange_curr = ObjRowsIdx[n] > AppConfig.get("ImgCrop_edgeTreshold_HorizBottom", 0.05).asDouble(); // edgeTreshold_HorizBottom;
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
			isInRange_curr = ObjColsIdx[n] > AppConfig.get("ImgCrop_edgeTreshold_Vert", 0.05).asDouble(); // edgeTreshold_Vert;
			if (!isInRange_prev & isInRange_curr) {
				VertEdge[0] = n;
				break;
			}
			isInRange_prev = isInRange_curr;
		}
		// start from right to find last element within threshold
		isInRange_prev = false;
		for (int n = ObjColsIdx.size() - 1; n > 0; n--) {
			isInRange_curr = ObjColsIdx[n] > AppConfig.get("ImgCrop_edgeTreshold_Vert", 0.05).asDouble(); // edgeTreshold_Vert;
			if (!isInRange_prev & isInRange_curr) {
				VertEdge[1] = n;
				break;
			}
			isInRange_prev = isInRange_curr;
		}

		//Mat fullimage = imread(AppConfig.get("ColorImg" + to_string(camNum) + "_name", "").asString());
		// check if cropping is within image size
		if (VertEdge[0] < 0 | VertEdge[1] > fullimage.cols | HorizEdge[0] < 0 | HorizEdge[1] > fullimage.rows) {
			cout << "Crop edges outside of image boudaries";
			return emptyMat;
		}
		Mat cropedImage = fullimage(Rect(VertEdge[0], HorizEdge[0], VertEdge[1] - VertEdge[0], HorizEdge[1] - HorizEdge[0]));
		Mat croppedFiltImage = GreenScreenFilter(cropedImage, AppConfig.get("chromaKeySetting", "Disabled").asString());
		//imwrite(AppConfig.get("ColorCropImg" + to_string(camNum) + "_name", "").asString(), croppedFiltImage);
		if (debugShow) {
			imshow("cropped", cropedImage);
			waitKey(0);
		}
		return croppedFiltImage;
		//fullimage.release();
		//cropedImage.release();
		//croppedFiltImage.release();
	}
	
	Mat CropAndGetTopCamImg(Mat fullimage, Mat dept2colorhMat, std::vector<uint16_t> data, Json::Value AppConfig, bool debugShow)
	{
		Mat emptyMat;
		//***********************************************
		int FrameMarginSides_Left = AppConfig.get("FrameMarginLRSides", 0).asInt();
		int FrameMarginSides_Right = dept2colorhMat.cols - AppConfig.get("FrameMarginLRSides", 0).asInt();
		cout << "LR Sides margin is: ";
		cout << AppConfig.get("FrameMarginLRSides", 0).asInt() << endl;
		int FrameMarginSides_Top = AppConfig.get("FrameMarginTBSides", 0).asInt();
		int FrameMarginSides_Bottom = dept2colorhMat.rows - AppConfig.get("FrameMarginTBSides", 0).asInt();
		cout << "TB Sides margin is: ";
		cout << AppConfig.get("FrameMarginTBSides", 0).asInt() << endl;

		for (int n = 0; n < dept2colorhMat.rows; n++) {
			for (int col = 0; col < dept2colorhMat.cols; col++) {
				// margins
				if (n < FrameMarginSides_Top | n > FrameMarginSides_Bottom
					| col <FrameMarginSides_Left | col > FrameMarginSides_Right) {
					data[n * dept2colorhMat.cols + col] = 0;
				}
				// zero valued pixels
				else if (data[dept2colorhMat.cols * n + col] == 0) {
					data[n * dept2colorhMat.cols + col] = 0;
				}
			}
		}
		// saturate all the pixel with zero value and farther away than 1100mm
		for (int n = 0; n < data.size(); n++) {
			if (data[n] >= 1100) {
				data[n] = 0;
			}
		}
		if (debugShow) {
			cv::Mat DepthImgSaturated = cv::Mat(dept2colorhMat.rows, dept2colorhMat.cols, CV_16U);
			memcpy(DepthImgSaturated.data, data.data(), data.size() * sizeof(uint16_t));
			cv::imshow("DepthImgSaturated", DepthImgSaturated);
			waitKey(0);
		}

		////***********************************************
		//// get the distance of the closest object
		//uint16_t PixelNum = 200;
		//std::vector<uint16_t> dataTemp = data;
		//std::sort(dataTemp.begin(), dataTemp.end());
		//uint16_t MinDistObj = std::accumulate(dataTemp.begin(), dataTemp.begin() + PixelNum, 0) / PixelNum;

		////***********************************************
		////set to zero every pixel at different distance of MinDistObj + -Delta
		//uint16_t DeltaDistance = AppConfig.get("DepthDelta_mm", 75).asInt();

		//std::vector<uint16_t> DepthImgWithinDistance = data;
		//for (int n = 0; n < data.size(); n++) {
		//	if (data[n] < MinDistObj - DeltaDistance | data[n] > MinDistObj + DeltaDistance) {
		//		DepthImgWithinDistance[n] = 0;
		//	}
		//}
		//// show
		//if (debugShow) {
		//	cv::Mat DepthImgClosestObj = Mat(dept2colorhMat.rows, dept2colorhMat.cols, CV_8UC1);
		//	//DepthImgClosestObj.data = *DepthImgWithinDistance;
		//	for (int n = 0; n < dept2colorhMat.rows * dept2colorhMat.cols; n++) {
		//		*(DepthImgClosestObj.data + n) = DepthImgWithinDistance[n];
		//	}
		//	//cv::Mat DepthImgClosestObj = cv::Mat(dept2colorhMat.rows, dept2colorhMat.cols, CV_16U);
		//	//memcpy(DepthImgClosestObj.data, DepthImgWithinDistance.data(), DepthImgWithinDistance.size() * sizeof(uint16_t));

		//	cv::imshow("DepthImgClosestObj", DepthImgClosestObj);
		//	waitKey(0);
		//}

		////***********************************************
		//// detect edges
		////double edgeTreshold_Vert = 0.2;
		////double edgeTreshold_HorizTop = 0.4;
		////double edgeTreshold_HorizBottom = 0.7;

		std::vector<uint16_t> DepthImgWithinDistance = data;
		double TocCamThreshold = 0.2;

		int count;
		std::vector<double> ObjRows;
		for (int n = 0; n < dept2colorhMat.rows; n++) {
			count = 0;
			//ObjRows.push_back(count_if(DepthImgWithinDistance.begin() + (dept2colorhMat.cols*n), DepthImgWithinDistance.begin() + (n + 1) * dept2colorhMat.cols, IsNotZero));
			for (int col = 0; col < dept2colorhMat.cols; col++) {
				if (DepthImgWithinDistance[dept2colorhMat.cols * n + col] != 0) {
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
			isInRange_curr = ObjRowsIdx[n] > TocCamThreshold; // AppConfig.get("ImgCrop_edgeTreshold_HorizTop", 0.05).asDouble(); // edgeTreshold_HorizTop;
			if (!isInRange_prev & isInRange_curr) {
				HorizEdge[0] = n;
				break;
			}
			isInRange_prev = isInRange_curr;
		}
		// start from the bottom to get last element within threshold
		isInRange_prev = false;
		for (int n = ObjRowsIdx.size() - 1; n > 0; n--) {
			isInRange_curr = ObjRowsIdx[n] > TocCamThreshold; // AppConfig.get("ImgCrop_edgeTreshold_HorizBottom", 0.05).asDouble(); // edgeTreshold_HorizBottom;
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
			isInRange_curr = ObjColsIdx[n] > TocCamThreshold; // AppConfig.get("ImgCrop_edgeTreshold_Vert", 0.05).asDouble(); // edgeTreshold_Vert;
			if (!isInRange_prev & isInRange_curr) {
				VertEdge[0] = n;
				break;
			}
			isInRange_prev = isInRange_curr;
		}
		// start from right to find last element within threshold
		isInRange_prev = false;
		for (int n = ObjColsIdx.size() - 1; n > 0; n--) {
			isInRange_curr = ObjColsIdx[n] > TocCamThreshold; // AppConfig.get("ImgCrop_edgeTreshold_Vert", 0.05).asDouble(); // edgeTreshold_Vert;
			if (!isInRange_prev & isInRange_curr) {
				VertEdge[1] = n;
				break;
			}
			isInRange_prev = isInRange_curr;
		}

		//Mat fullimage = imread(AppConfig.get("ColorImg" + to_string(camNum) + "_name", "").asString());
		// check if cropping is within image size
		if (VertEdge[0] < 0 | VertEdge[1] > fullimage.cols | HorizEdge[0] < 0 | HorizEdge[1] > fullimage.rows) {
			cout << "Crop edges outside of image boudaries";
			return emptyMat;
		}
		Mat cropedImage = fullimage(Rect(VertEdge[0], HorizEdge[0], VertEdge[1] - VertEdge[0], HorizEdge[1] - HorizEdge[0]));
		Mat croppedFiltImage = GreenScreenFilter(cropedImage, AppConfig.get("chromaKeySetting", "Disabled").asString());
		//imwrite(AppConfig.get("ColorCropImg" + to_string(camNum) + "_name", "").asString(), croppedFiltImage);
		if (debugShow) {
			imshow("cropped", cropedImage);
			waitKey(0);
		}
		return croppedFiltImage;
		//fullimage.release();
		//cropedImage.release();
		//croppedFiltImage.release();
	}

	Mat GreenScreenFilter(Mat img, string chromaKeySetting) {
		Mat hsv, mask, imgFilt;
		cvtColor(img, hsv, COLOR_BGR2HSV);

		if (chromaKeySetting == "Green") {
			// create a mask for green
			inRange(hsv, Scalar(65, 60, 60), Scalar(80, 255, 255), mask);
			bitwise_or(img, img * 0 + 255, imgFilt, mask);
			imgFilt = img + (imgFilt);
			return imgFilt;
		}
		else if (chromaKeySetting == "White") {
			// create a mask for white
			inRange(hsv, Scalar(0, 0, 120), Scalar(255, 38, 255), mask);
			bitwise_or(img, img * 0 + 255, imgFilt, mask);
			imgFilt = img + (imgFilt);
			return imgFilt;
		}
		else {
			return img;
		}
	}

}
