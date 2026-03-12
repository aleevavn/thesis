#include <iostream>
#include <fstream>
#include <string>
#include <cmath>
#include <cstdlib>
#include <ctime>
#include <iomanip>
#include <omp.h>

using namespace std;

//размерность
int L = 128;
int M = 128;

//функция вычисления нормы
float Norm(float ***u, int n, int o)//передаем массив 
{
	float Epsilon = 0.00001;//Точность вычислений
	float Result = 0.0;//для сравнения норм
	for (int i = 0; i < L; i++)
	{
		for (int j = 0; j < M; j++)
		{
			double ValueOfCurrentIteration(fabs(u[n][i][j] - u[o][i][j]));//вычисляем норму
			if (ValueOfCurrentIteration > Result) Result = ValueOfCurrentIteration;//если текущая норма больше, чем максимально найденная ранее, то заменяем на текущую
		}
	}
	return Result;//возвращаем максимально найденную норму
}

int main() {
	unsigned int V = 3;
	int n = 1;
	int o = 1;
	int N;
	N = L * M;
	int i, j;
	float *** u = new float**[V];
	for (int i1 = 0; i1 < V; i1++)
	{
		u[i1] = new float*[L];
		{
			for (int j1 = 0; j1 < L; j1++)
				u[i1][j1] = new float[M];
		}
	}
	const float Epsilon = 0.00001;//точность вычислений
	for (int i = 0; i < L; i++)
	{
		for (int j = 0; j < M; j++)
		{
			u[0][i][j] = 0;//заполняем начальные приближения
		}
	}
	int a, b, c, d, f, e;//по формуле веса
	a = 1;
	b = 1;
	c = 1;
	d = 1;
	f = 0;
	e = 4;
	float norm;
	float start_time = clock() / (float)CLOCKS_PER_SEC;
	do
	{
		int o = 1;

	
		#pragma omp parallel for schedule(static) num_threads(24)
		for (int i = 0; i < L; i++)
		{
			#pragma omp parallel for schedule(static) num_threads(1)
			for (int j = 0; j < M; j++)
			{
				if (n == 0) o = 2;
				if (n != 0) o = (n - 1) % 3;
				if (i == 0 && j == 0)
				{
					u[n][i][j] = (f + a * 1 + b * 1 + c * u[o][i + 1][j] + d * u[o][i][j + 1]) / e;
				}
				else if (i == 0 && j == M - 1)
				{
					u[n][i][j] = (f + a * 1 + b * u[o][i][j - 1] + c * u[o][i + 1][j] + d * 1) / e;
				}
				else if (i == 0 && j > 0 && j < M - 1)
				{
					u[n][i][j] = (f + a * 1 + b * u[o][i][j - 1] + c * u[o][i + 1][j] + d * u[o][i][j + 1]) / e;
				}
				else if (i == L - 1 && j == 0)
				{
					u[n][i][j] = (f + a * u[o][i - 1][j] + b * 1 + c * 1 + d * u[o][i][j + 1]) / e;
				}
				else if (i == L - 1 && j == M - 1)
				{
					u[n][i][j] = (f + a * u[o][i - 1][j] + b * u[o][i][j - 1] + c * 1 + d * 1) / e;
				}
				else if (i == L - 1 && j > 0 && j < M - 1)
				{
					u[n][i][j] = (f + a * u[o][i - 1][j] + b * u[o][i][j - 1] + c * 1 + d * u[o][i][j + 1]) / e;
				}
				else if (i != 0 && i != L - 1 && j == 0)
				{
					u[n][i][j] = (f + a * u[o][i - 1][j] + b * 1 + c * u[o][i + 1][j] + d * u[o][i][j + 1]) / e;
				}
				else if (i != 0 && i != L - 1 && j == M - 1)
				{
					u[n][i][j] = (f + a * u[o][i - 1][j] + b * u[o][i][j - 1] + c * u[o][i + 1][j] + d * 1) / e;
				}
				else if (i > 0 && i < L - 1 && j > 0 && j < M - 1)
				{
					u[n][i][j] = (f + a * u[o][i - 1][j] + b * u[o][i][j - 1] + c * u[o][i + 1][j] + d * u[o][i][j + 1]) / e;
				}
			}
		}


		norm = Norm(u, n, o);
		if (n % 3 == 2) n = 0;
		else n = n + 1;
	} while (norm > Epsilon);
	float end_time = clock() / (float)CLOCKS_PER_SEC;
	cout << "time = " << end_time - start_time << endl;
	delete[] u;
	system("pause");
	return 0;
}



