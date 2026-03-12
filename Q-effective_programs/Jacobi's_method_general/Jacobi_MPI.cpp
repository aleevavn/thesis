#include <mpi.h>
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <string.h>

#include "helpers.h"

#define upTag   0
#define downTag 1
#define other 2
       
float* JacobiMPI(int n, int maxIter, int argc, char *argv[]) {

    int procCount, id, height;
    float localDiff = 0.0, globalDiff = 0.0;

    MPI_Status status;
    MPI_Request rqSendUp, rqSendDown;

    int upR, downR;
    MPI_Init( &argc, &argv );
    MPI_Comm_rank( MPI_COMM_WORLD, &id );
    MPI_Comm_size( MPI_COMM_WORLD, &procCount );

    if(n % procCount != 0) 
	{
        printf("Error procCount = %d", procCount);
        exit(EXIT_FAILURE);
    }

    height = n / procCount;

    float **grid = (float**) malloc((height + 2) * sizeof(float*));
    float **new = (float**) malloc((height + 2) * sizeof(float*));

    float *globalGrid;
    float *globalNew;

    for(int j = 0; j < height + 2; j++) 
	{
        grid[j] = (float*) malloc((n + 2) * sizeof(float));
        new[j] = (float*) malloc((n + 2) * sizeof(float));
    }
    
    if(id == 0) 
	{
        globalGrid = (float*) malloc((n + 2) * height * procCount * sizeof(float));
        globalNew = (float*) malloc((n + 2) * height * procCount * sizeof(float));
    }

    initialize(grid, n, height, procCount, id);
    initialize(new, n, height, procCount, id);

	double startTime, endTime;
	startTime = omp_get_wtime();
	MPI_Barrier(MPI_COMM_WORLD);
	#pragma omp parallel for
	for(int t = 0; t < maxIter; t = t + 2) 
	{
		#pragma omp parallel for
		for(int i = 1; i <= height; i++) 
		{
			#pragma omp parallel for
			for(int j = 1; j <= n; j++) 
			{
				new[i][j] = (grid[i - 1][j] + grid[i + 1][j] + grid[i][j - 1] + grid[i][j + 1]) * 0.25;
			}
		}

        if(id > 0) 
		{
            upR = MPI_Isend(new[1], n + 2, MPI_FLOAT, id - 1, upTag, MPI_COMM_WORLD, &rqSendUp);
            handle_mpi_error(upR);
        }
        if(id < procCount - 1) 
		{
            downR = MPI_Isend(new[height], n + 2, MPI_FLOAT, id + 1, downTag, MPI_COMM_WORLD, &rqSendDown);
            handle_mpi_error(downR);
        }
#pragma omp parallel for
        for(int i = 2; i <= height - 1; i++) 
		{
			#pragma omp parallel for
			for(int j = 1; j <= n; j++) 
			{
                grid[i][j] = (new[i - 1][j] + new[i + 1][j] + new[i][j - 1] + new[i][j + 1]) * 0.25;
            }
        }

        if(id > 0)
            MPI_Wait(&rqSendUp, &status);
        if(id < procCount - 1)
            MPI_Wait(&rqSendDown, &status);

        if(id < procCount - 1) 
		{
            memcpy(new[height + 1], grid[height + 1], sizeof(float) * (n + 2));
            upR = MPI_Recv(new[height + 1], n + 2, MPI_FLOAT, id + 1, upTag, MPI_COMM_WORLD, &status);
            handle_mpi_error(upR);
        }
        if(id > 0) 
		{
            memcpy(new[0], grid[0], sizeof(float) * (n + 2));
            downR = MPI_Recv(new[0], n + 2, MPI_FLOAT, id - 1, downTag, MPI_COMM_WORLD, &status);
            handle_mpi_error(downR);
        }
        
		for(int j = 1; j <= n; j++) 
		{
            grid[1][j] = (new[0][j] + new[2][j] + new[1][j - 1] + new[1][j + 1]) * 0.25;
            grid[height][j] = (new[height - 1][j] + new[height + 1][j] + new[height][j - 1] + new[height][j + 1]) * 0.25;
        }
    }

    MPI_Gather(grid[1], (n + 2) * height, MPI_FLOAT, globalGrid, (n + 2) * height, MPI_FLOAT, 0, MPI_COMM_WORLD);
    MPI_Gather(new[1], (n + 2) * height, MPI_FLOAT, globalNew, (n + 2) * height, MPI_FLOAT, 0, MPI_COMM_WORLD);

    for(int i = 1; i <= height; i++) 
	{
        for(int j = 1; j <= n; j++) 
		{
            localDiff = fmax(localDiff, fabs(grid[i][j] - new[i][j]));
        }
    }
    
    MPI_Reduce(&localDiff, &globalDiff, 1, MPI_FLOAT, MPI_MAX, 0, MPI_COMM_WORLD);
        
    if(id == 0) 
	{
        printf("New :\n");
        print_buffer(globalNew, (n + 2) * n, n + 2);
        printf("Grid :\n");
        print_buffer(globalGrid, (n + 2) * n, n + 2);
        printf("%f\n", globalDiff);
    }

	MPI_Finalize();
	endTime = omp_get_wtime();
	time=(endTime-startTime);
	return globalGrid;
}

int main(int argc, char *argv[]) 
{
	int n = atoi(argv[1]);
	int maxIter = atoi(argv[2]);

	if(argc != 3 || n < 3 || maxIter < 1) 
	{
		printf("Size error");
		exit(EXIT_FAILURE);
	}
	JacobiMPI(n, maxIter, argc, argv);
	return 0;
}
