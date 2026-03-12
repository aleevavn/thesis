#include "stdafx.h"
#include <stdio.h>
#include <tchar.h>
#include <cstdio>
#include <fstream>
#include <iostream>
#include <ctime>
#include <omp.h>

using namespace std;

int N;

inline float Abs(float value) {
	if (value >= 0) {
		return value;
	}
	else {
		return -value;
	}
}

inline bool converge(float *x, float *p)
{
	float eps = 0.001;
	for (int i = 0; i < N; i++)
	{
		if (x[i] - p[i] > eps) {
			return false;
		}
	}
	return true;
}

int main()
{
	srand(time(0));
	int N = 30000;
	int M;
	
	float **input = new float *[N];
	float **A = new float *[N];
	float *B = new float[N];

	//read input
	for (int i = 0; i < N; i++)
	{
		input[i] = new float[N + 1];
		A[i] = new float[N];

		for (int j = 0; j < N + 1; j++)
		{
			input[i][j] = (rand() % 100 - 50)*0.5;
		}
	}

	//diagonal predominance
	for (int j = 0; j < N - 1; j++) {
		float max = Abs(input[j][j]);
		int maxN = j;

		for (int i = j + 1; i < N; i++) {

			if (Abs(input[i][j]) > max) {
				maxN = i;
				max = Abs(input[i][j]);
			}
		}

		for (int k = 0; k < N + 1; k++) {
			float temp = input[j][k];
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
	float *x = new float[N];
	float *p = new float[N];

	for (int i = 0; i < N; i++) {
		x[i] = 0;
	}

	//main cycle
	float start_time = clock() / (float)CLOCKS_PER_SEC;
	do
	{
		//save previous X
		for (int i = 0; i < N; i++)
			p[i] = x[i];

		//1. сдваивание
		for (int i = 0; i < N; i++)
		{
			float sum = B[i] / A[i][i];
			int half = (N - (i + 1)) / 2;

#pragma omp parallel for num_threads(7) reduction(+:sum)
			for (int j = i + 1; j < half; j++) {
				sum += p[j] * A[i][j] / A[i][i] + p[N - j] * A[i][N - j] / A[i][i];
			}
			if ((N - (i + 1)) % 2 != 0) {
				sum += p[half] * A[i][half] / A[i][i];
			}
			x[i] = sum;
		}

		//2. подстановка
		for (int i = 0; i < N - 1; i++)
		{
#pragma omp parallel for num_threads(7)
			for (int j = i + 1; j < N; j++)
				x[j] += x[i] * A[j][i] / A[j][j];
		}
	} while (!converge(x, p));
	float end_time = clock() / (float)CLOCKS_PER_SEC;

	cout << "time = " << end_time - start_time << endl;

	system("pause");
	return 0;
}
