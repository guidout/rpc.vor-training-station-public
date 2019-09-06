// links
// https://stackoverflow.com/questions/57222190/how-to-convert-k4a-image-t-to-opencv-matrix-azure-kinect-sensor-sdk
// https://www.deciphertechnic.com/install-opencv-with-visual-studio/
// https://stackoverflow.com/questions/16312904/how-to-write-a-float-mat-to-a-file-in-opencv/16314041


//cv::Mat colorMat(k4a_image_get_height_pixels(image), k4a_image_get_width_pixels(image), (((0) & ((1<3)-1)) + (((4)-1)<<3)), (void*)k4a_image_get_buffer(image), cv::Mat::AUTO_STEP);

//std::string file_name;
//file_name = "test.txt";
//std::ofstream ofs(file_name);
//std::stringstream ss;
/*for (int i = 0; i < imgWidthTemp * imgHeightTemp * 4; i++)
			{
				imgBytes[i] = image_data[i];
			}
			for (int i = 0; i < imgWidthTemp * imgHeightTemp * 4; i++)
			{
				ss << (int)imgBytes[i] << std::endl;
			}*/
			//for (int i = 0; i < imgWidthTemp * imgHeightTemp; i++)
			//{
			//	ss << (uint16_t)image_data[i*4] << "," << (uint16_t)image_data[i * 4 +1] << "," << (uint16_t)image_data[i * 4 +2] << "," << (uint16_t)image_data[i * 4 +3] << std::endl;
			//	//printf(ss.str().c_str());
			//}
			//
			//std::ofstream ofs_text(file_name, std::ios::out | std::ios::app);
			//ofs_text.write(ss.str().c_str(), (std::streamsize)ss.str().length());


//k4a_float2_t* table_data = (k4a_float2_t*)(void*)k4a_image_get_buffer(image);
			//double imgBuf;
			//int imgWidth_pixels = k4a_image_get_width_pixels(image);
			//int imgHeight_pixels = k4a_image_get_height_pixels(image);
		/*
			uint8_t imgBuf = *(k4a_image_get_buffer(image)+2);
			uint32_t size_t = k4a_image_get_size(image);
			int imgWidth = k4a_image_get_width_pixels(image);
			int imgHeight = k4a_image_get_height_pixels(image);
			int imgStride =	k4a_image_get_stride_bytes(image);
			k4a_float2_t* table_data = (k4a_float2_t*)(void*)k4a_image_get_buffer(image);
			//res:2160x3840
			uint32_t my_new_image[100];
			std::string file_name;
			file_name = "test";
			std::ofstream ofs(file_name);
			std::stringstream ss;
				//for (int i = 0; i < imgWidthTemp * imgHeightTemp; i++)
			for (int i = 0; i < 10; i++)
					//my_new_image[i] = *(k4a_image_get_buffer(image) + i);
				{
					//ss << (float)point_cloud_data[i].xyz.x << " " << (float)point_cloud_data[i].xyz.y << " "
					//	<< (float)point_cloud_data[i].xyz.z << std::endl;
					ss << (float)(*(k4a_image_get_buffer(image) + i)) << std::endl;
					printf(ss.str().c_str());
				}
				std::ofstream ofs_text(file_name, std::ios::out | std::ios::app);
				ofs_text.write(ss.str().c_str(), (std::streamsize)ss.str().length());
			k4a_image_release(image);
		}
		*/