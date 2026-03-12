#include "mpi.h"
#include <stdio.h>
#include <cstdio>
#include <fstream>
#include <iostream>
#include <ctime>
#include <omp.h>


using namespace std;

int N;

inline double Abs(double value) {
	if (value >= 0) {
		return value;
	}
	else {
		return -value;
	}
}

inline bool converge(double *x, double *p)
{
	double eps = 0.001;
	for (int i = 0; i < N; i++)
	{
		if (x[i] - p[i] > eps) {
			return false;
		}
	}
	return true;
}

int main(int argc, char* argv[])
{
	srand(time(0));
	int N = 30000;
	int M;
	
	double **input = new double *[N];
	double **A = new double *[N];
	double *B = new double[N];

	//reading input
	for (int i = 0; i < N; i++)
	{
		input[i] = new double[N + 1];
		A[i] = new double[N];

		for (int j = 0; j < N + 1; j++)
		{
			input[i][j] = (rand() % 100 - 50)*0.5;
		}
	}


	//diagonal predominance
	for (int j = 0; j < N - 1; j++) {
		double max = Abs(input[j][j]);
		int maxN = j;

		for (int i = j + 1; i < N; i++) {

			if (Abs(input[i][j]) > max) {
				maxN = i;
				max = Abs(input[i][j]);
			}
		}

		for (int k = 0; k < N + 1; k++) {
			double temp = input[j][k];
			input[j][k] = input[maxN][k];
			input[maxN][k] = temp;
		}
	}


	//building A[N][N] and B[N] matrices
	for (int i = 0; i < N; i++) {
		for (int j = 0; j < N; j++) {
			A[i][j] = input[i][j];
		}
		B[i] = input[i][N];
	}

	//x - current X
	//p - previous X
	double *x = new double[N];
	double *p = new double[N];

	for (int i = 0; i < N; i++) {
		x[i] = 0;
	}

	//main cycle
	float start_time = clock() / (float)CLOCKS_PER_SEC;
	do
	{
		for (int i = 0; i < N; i++)
			p[i] = x[i];
		
  int myid;
    int ranknum = 0;
    MPI_Init(&argc, &argv);                 
    MPI_Comm_rank(MPI_COMM_WORLD, &myid);           
    MPI_Comm_size(MPI_COMM_WORLD, &ranknum);
		//1. сдваивание
		for (int i = 0; i < N; i++)
		{
			double sum = B[i] / A[i][i];
			int half = (N - (i + 1)) / 2;

  

#pragma omp parallel for reduction(+:sum)
			for (int j = i + 1; j < half; j++) {
				x[i] += p[j] * A[i][j] / A[i][i] + p[N - j] * A[i][N - j] / A[i][i];
			}
			if ((N - (i + 1)) % 2 != 0) {
				sum += p[half] * A[i][half] / A[i][i];
			}
			x[i] = sum;
		}

		//2. подстановка 
		for (int i = 0; i < N - 1; i++)
		{
#pragma omp parallel for
			for (int j = i + 1; j < N; j++)
				x[j] += x[i] * A[j][i] / A[j][j];
		}
	} while (!converge(x, p));
    MPI_Finalize();
	float end_time = clock() / (float)CLOCKS_PER_SEC;

	cout << "time = " << end_time - start_time << endl;
	return 0;
}


