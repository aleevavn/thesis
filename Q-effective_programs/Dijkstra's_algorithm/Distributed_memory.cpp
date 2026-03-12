#include <iostream>
#include <vector>
#include <string>
#include "mpi.h"
#include "omp.h"
using namespace std;
std::vector<short> RandomMatrix(int size)
{
	std::vector<short>  matrix(size * size, 0);
	srand(int(time(NULL)));
#pragma omp parallel  for 
	for (int i = 0; i < size; i++)
		for (int j = 0; j < size; j++)
			if (i != j)
				matrix[i * size + j] = (rand() + omp_get_thread_num()) % 100;
	return matrix;
}
void dijkstra(int size, int world_size, int world_rank)
{
	size = size - size % world_size;
	vector<short> eTransformed = RandomMatrix(size);
#pragma omp parallel for
	for (int i = 0; i < size; i++)
		eTransformed[i * size + i] = -1;
#pragma omp parallel for
	for (int i = 0; i < size * size; i++)
		if (eTransformed[i] == 0)
			eTransformed[i] = 999;
	MPI_Barrier(MPI_COMM_WORLD);
	double start_time = MPI_Wtime();
	double finish_time;
	vector<unsigned short> distances_prev(size, 0);
	vector<unsigned short> distances_curr(size, 0);
	bool FinishInitialized = false;
#pragma omp parallel  for 
	for (int i = 1; i < size; i++)
		distances_curr[i] = eTransformed[i]; 
	vector<short>distancesBuffer(size * size / world_size, 0);
	vector<unsigned short>distancesReducedBuffer(size, 999);
	while (true)
	{
		MPI_Barrier(MPI_COMM_WORLD);
		if (world_rank == 0)
		{
			if (distances_curr == distances_prev)
			{
				finish_time = MPI_Wtime();
				FinishInitialized = true;
			}
			else
				distances_prev = distances_curr;
		}
		MPI_Bcast(&FinishInitialized, 1, MPI_INT, 0, MPI_COMM_WORLD);
		if (FinishInitialized)
			break;
		MPI_Bcast(distances_curr.data(), size, MPI_UNSIGNED_SHORT, 0, MPI_COMM_WORLD);
		std::vector<int> displs(world_size, 0);
		std::vector<int> sendcounts(world_size, 0);
		int sum = 0;
		int rem = size % world_size;
		for (int i = 0; i < world_size; i++)
		{
			sendcounts[i] = size*size / world_size;
			if (rem > 0) {
				sendcounts[i]++;
				rem--;
			}

			displs[i] = sum;
			sum += sendcounts[i];

		}
		MPI_Scatterv(eTransformed.data(), sendcounts.data(), displs.data(), MPI_SHORT, distancesBuffer.data(), size*size, MPI_SHORT, 0, MPI_COMM_WORLD);
		for (int k = 0; k < size / world_size; k++) 
		{
			short rootDistance = 0;
			int rootNumber = -1;
#pragma omp parallel for
			for (int i = 0; i < size; i++)
				if (distancesBuffer[k * size + i] == -1)
				{
					distancesBuffer[k * size + i] = distances_curr[i];
					rootDistance = distances_curr[i];
					rootNumber = i;
				}
#pragma omp parallel for
			for (int i = 0; i < size; i++)
				distancesBuffer[k * size + i] += rootDistance; 
#pragma omp parallel for
			for (int i = 0; i < size; i++)
				distancesReducedBuffer[i] = ((distancesBuffer[k * size + i] < distancesReducedBuffer[i]))
				? (distancesBuffer[k * size + i])
				: distancesReducedBuffer[i];
		}
		MPI_Reduce(distancesReducedBuffer.data(), distances_curr.data(), size, MPI_UNSIGNED_SHORT, MPI_MIN, 0, MPI_COMM_WORLD);
	}
	if (world_rank == 0)
	{
		cout << finish_time - start_time << ", ";
	}
}
int main(int argc, char** argv)
{
	int tries = 10;
	int world_size = 0;
	int world_rank = 0;
	omp_set_dynamic(0);
	MPI_Init(NULL, NULL);
	MPI_Comm_size(MPI_COMM_WORLD, &world_size);
	MPI_Comm_rank(MPI_COMM_WORLD, &world_rank);
	for (int t = 5; t > 1; t--)
	{
		for (int k = 0; k < tries; k++)
			dijkstra(t, world_size, world_rank);
		if (world_rank == 0)
			cout << endl;
	}
	MPI_Finalize();

}

