#include <mpi.h>
#include <omp.h>
#include <iostream>
#include <vector>
#include <algorithm>
#include <ctime>
#include <cstdlib>

using namespace std;

class ShellSortMPI()
{
	void generateRandomArray(vector<int>& arr, int size) {
		arr.resize(size);
		for (int i = 0; i < size; ++i) {
			arr[i] = rand() % 1000; // случайные числа от 0 до 999
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
				arr[i] = rand() % 1000; // случайные числа от 0 до 999
			}
		}
	}

	void merge(std::vector<int>& left, std::vector<int>& right, std::vector<int>& bars) {
		int nL = left.size();
		int nR = right.size();
		int i = 0, j = 0, k = 0;

		while (j < nL && k < nR) {
			if (left[j] < right[k]) {
				bars[i] = left[j];
				j++;
			}
			else {
				bars[i] = right[k];
				k++;
			}
			i++;
		}
		while (j < nL) {
			bars[i] = left[j];
			j++; i++;
		}
		while (k < nR) {
			bars[i] = right[k];
			k++; i++;
		}
	}

	void mergeBlocks(std::vector<int>& data, int block_size, int num_blocks) {
		std::vector<int> merged_data(data.size());
		std::vector<int> left(block_size);
		std::vector<int> right(block_size);

		for (int i = 0; i < num_blocks - 1; i++) {
			std::copy(data.begin() + i * block_size, data.begin() + (i + 1) * block_size, left.begin());
			std::copy(data.begin() + (i + 1) * block_size, data.begin() + (i + 2) * block_size, right.begin());

			merge(left, right, merged_data);

			std::copy(merged_data.begin(), merged_data.begin() + (i + 2) * block_size, data.begin());
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

	int main(int argc, char* argv[]) {
		MPI_Init(&argc, &argv);

		int rank, size;
		MPI_Comm_rank(MPI_COMM_WORLD, &rank);
		MPI_Comm_size(MPI_COMM_WORLD, &size);

		srand(time(NULL) + rank);

		const int array_size = 800000000;
		vector<int> local_array(array_size);
		vector<vector<int> > global_arrays(3);

		if (rank == 0) {
			generateRandomArray(global_arrays[0], array_size);
			generateReversedArray(global_arrays[1], array_size);
			generatePartiallySortedArray(global_arrays[2], array_size);
		}

		for (int i = 0; i < 3; ++i) {
			// Scatter the global arrays to local arrays
			MPI_Scatter(global_arrays[i].data(), array_size / size, MPI_INT,
						local_array.data(), array_size / size, MPI_INT, 0, MPI_COMM_WORLD);

			// Start measuring sorting time
			double start_time = MPI_Wtime();

			// Perform shell sort on the local array
			shellSort(local_array);

			// Stop measuring sorting time
			double end_time = MPI_Wtime();

			// Gather sorted local arrays back to global arrays
			MPI_Gather(local_array.data(), array_size / size, MPI_INT,
					   global_arrays[i].data(), array_size / size, MPI_INT, 0, MPI_COMM_WORLD);

			if (rank == 0) {
				mergeBlocks(global_arrays[i], array_size / size, size);
				//cout << "Sorted array " << i + 1 << ": ";
				//for (size_t j = 0; j < global_arrays[i].size(); ++j) {
					//int num = global_arrays[i][j];
					//cout << num << " ";
				//}
				//cout << endl;
				cout << "Time taken for sorting: " << ((end_time - start_time) * 1000) << " milliseconds" << endl;
			}
		}

		MPI_Finalize();
		return 0;
	}
}