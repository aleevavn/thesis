#include <mpi.h> 
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

//функци€ вычислени€ нормы
float Norm(float ***u, int n, int o)//передаем массив 
{
	float Epsilon = 0.00001;//“очность вычислений
	float Result = 0.0;//дл€ сравнени€ норм
	for (int i = 0; i < L; i++)
	{
		for (int j = 0; j < M; j++)
		{
			double ValueOfCurrentIteration(fabs(u[n][i][j] - u[o][i][j]));//вычисл€ем норму
			if (ValueOfCurrentIteration > Result) Result = ValueOfCurrentIteration;//если текуща€ норма больше, чем максимально найденна€ ранее, то замен€ем на текущую
		}
	}
	return Result;//возвращаем максимально найденную норму
}

int main(int argc, char *argv[]) {

	unsigned int V = 3;//кол-во шагов
	int n = 1;//начинаем с первого шага
	int o = 1;
	int N;
	N = L * M;
	int i, j;
	float *** u = new float**[V];//создаем трехмерный массив
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
			u[0][i][j] = 0;//заполн€ем начальные приближени€
		}
	}
	int a, b, c, d, f, e;//по формуле веса
	a = 1;
	b = 1;
	c = 1;
	d = 1;
	f = 0;
	e = 4;
	double startMPI, endMPI;
	float norm;//дл€ результата вызова функции Norm
	float start_time = clock() / (float)CLOCKS_PER_SEC;//начинаем считать врем€
	MPI_Init(&argc, &argv);//инициализаци€
	startMPI = MPI_Wtime();//врем€ MPI
	do
	{
		int rank, size;
		MPI_Comm_size(MPI_COMM_WORLD, &size);// оличество процессов в ком-муникаторе     
		MPI_Comm_rank(MPI_COMM_WORLD, &rank);//Ќомер (ранг) текущего про-цесса в коммуникаторе 
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
	endMPI = MPI_Wtime();//завершение подсчета времени с помощью MPI
	double resTime = endMPI - startMPI;
	resTime /= size;
	if (rank == 0) {
		resTime /= size;
		cout << "timeMPI = " << resTime << endl;//вывод времени с учетом кол-ва узлов
	}
	MPI_Finalize();//завершение 
	delete[] u;
	system("pause");
	return 0;
}
