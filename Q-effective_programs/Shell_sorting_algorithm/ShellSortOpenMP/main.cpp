#include <iostream>
#include <vector>
#include <algorithm>
#include <omp.h>
#include <chrono>

class ShellSortOpenMP()
{	
	void shellSort(std::vector<int>& arr) {
		int n = arr.size();

		#pragma omp parallel
		{
			for (int gap = n / 2; gap > 0; gap /= 2) {
				#pragma omp for
				for (int i = gap; i < n; i++) {
					int temp = arr[i];
					int j;
					#pragma omp simd reduction(+:j)
					for (j = i; j >= gap && arr[j - gap] > temp; j -= gap) {
						arr[j] = arr[j - gap];
					}
					arr[j] = temp;
				}
			}
		}
	}

	void generateRandomArray(vector<int>& arr, int size) {
		arr.resize(size);
		for (int i = 0; i < size; ++i) {
			arr[i] = rand() % 1000;
		}
	}

	void generateReversedArray(vector<int>& arr, int size) {
		arr.resize(size);
		for (int i = 0; i < size; ++i) {
			arr[i] = size - i;
		}
	}

	void generatePartiallySortedArray(vector<int>& arr, int size) {
		arr.resize(size);
		for (int i = 0; i < size; ++i) {
			if (i < size / 2) {
				arr[i] = i;
			} else {
				arr[i] = rand() % 1000;
			}
		}
	}
	
	void сheckArray(int array[], int length)
	{
		bool check;
		for (int i = 0; i < length; ++i)
		{
			if (array[i] <= array[i + 1])
			{
				check = true;
			}
		}
		if (check) cout << "\n Array successfully sorted";
		else cout << "\n Array not sorted";
	}

	int main() {
		int length = N;
		std::vector<int> randomArray(length);
		std::vector<int> reversedArray(length);
		std::vector<int> partiallySortedArray(length);
		
		generateRandomArray(randomArray, length);
		generateReversedArray(reversedArray, length);
		generatePartiallySortedArray(partiallySortedArray, length);

		auto start = std::chrono::monotonic_clock::now();
		shellSort(randomArray);
		auto end = std::chrono::monotonic_clock::now();
		std::cout << "Time taken to sort random array: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << " ms\n";

		start = std::chrono::monotonic_clock::now();
		shellSort(reversedArray);
		end = std::chrono::monotonic_clock::now();
		std::cout << "Time taken to sort reversed array: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << " ms\n";

		start = std::chrono::monotonic_clock::now();
		shellSort(partiallySortedArray);
		end = std::chrono::monotonic_clock::now();
		std::cout << "Time taken to sort partially sorted array: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << " ms\n";

		return 0;
	}
}