#include <json/json.h>
#include <stdio.h>
#include <malloc.h>
#include <string.h>
#include <k4a/k4a.h>
#include <iostream>
#include <string.h>

using namespace std;

int main(int argc, char** argv)
{
	k4a_device_t device = NULL;

	uint32_t device_count = k4a_device_get_installed_count();
	printf("Found %d connected devices:\n", device_count);
	for (uint8_t deviceIndex = 0; deviceIndex < device_count; deviceIndex++)
	{
		if (K4A_RESULT_SUCCEEDED != k4a_device_open(deviceIndex, &device))
		{
			printf("%d: Failed to open device\n", deviceIndex);
		}

		char* serial_number = NULL;
		size_t serial_number_length = 0;

		if (K4A_BUFFER_RESULT_TOO_SMALL != k4a_device_get_serialnum(device, serial_number, &serial_number_length))
		{
			printf("%d: Failed to get serial number length\n", deviceIndex);
			k4a_device_close(device);
			device = NULL;
			continue;
		}
		/*for (int n = 1; n < serial_number_length; n++) {
			cout << &serial_number_length[n];
		}*/
		/*serial_number = (char*)malloc(serial_number_length);
		char* serial = (char*)(void*)malloc(serial_number_length);
		std::cout << 1;*/
	}

}