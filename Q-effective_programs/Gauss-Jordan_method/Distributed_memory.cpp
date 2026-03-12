#include <mpi.h>
#include <stdio.h>
#include "omp.h"

#define SIZE 1000

double *allocationArrayTwo(int size);
void closeArrayTwo(double *arr, int size);
void InitSparseMatrix(double *matrix, int size);
double gauss_jordan_q(double *matrix, int size, int node, int k, bool *u);
void calcMatrixToNode(double *matrix, double *temp, int size, int k, int lead, bool *u);

//ВЫДЕЛЕНИЕ ПАМЯТИ ДЛЯ ХРЕНЕНИЯ ДВУМЕРНОГО МАССИВА
double *allocationArrayTwo(int size){
	double *temp;

	temp = new double [size * (size + 1)];

	return temp;
}

//ОСВОБОЖДЕНИЕ ПАМЯТИ, ВЫДЕЛЕННОЙ ПОД ДВУМЕРНЫЙ МАССИВ
void closeArrayTwo(double *arr, int size){
	//если память не занята, выход из процедуры
	if(arr == NULL)
		return;

	delete [] arr;
	arr = NULL;
}

//ИНИЦИАЛИЗАЦИЯ РАЗРЕЖЕННОЙ МАТРИЦЫ
void InitSparseMatrix(double *matrix, int size){
	double k = 1.0;

	for(int i = 0; i < size; i++){
		for(int j = 0; j < size + 1; j++){
			//главная диагональ
			if(i == j)
				matrix[i * (size + 1) + j] = 1.0;
			else
				//столбец свободных членов
				if(j == size)
					matrix[i * (size + 1) + j] = ++k;
				else
					//остальные элементы
					matrix[i * (size + 1) + j] = 0.0;
		}
	}
}

//ВЫВОД МАТРИЦЫ НА ЭКРАН
void PrintMatrix(double *matrix, int size){
	for(int i = 0; i < size; i++){
		for(int j = 0; j < size + 1; j++){
			printf("%.2lf ", matrix[i * (size + 1) + j]);
		}
		printf("\n");
	}
}

//КОПИРОВАНИЕ МАССИВА ИЗ in В out
void copyMatrix(double *in, double *out, int size){
	for(int i = 0; i < size; i++){
		for(int j = 0; j < size + 1; j++){
			out[i * (size + 1) + j] = in[i * (size + 1) + j];
		}
	}
}

/*
ВЫЧИСЛЕНИЕ ЗНАЧЕНИЙ МАССИВА НА ТЕКУЩЕМ УЗЛЕ
matrix - исходный и результирующий массив
temp - промежуточный массив
k - номер шага;
lead - индекс ведущего элемента
u - указатель на логический q-терм
*/
void calcMatrixToNode(double *matrix, double *temp, int size, int k, int lead, bool *u){
	for(int i = 0; i < size; i++){
		//если логический Q-терм = false, вычисление прекращается
		if(*u == false) break;

		for(int j = 0; j < size + 1; j++){
			//если логический Q-терм = false, вычисление прекращается
			if(*u == false) break;

			if(k == i){
				//для избежания получения отрицательного нуля
				if(matrix[k * (size + 1) + j] == 0.0){
					temp[i * (size + 1) + j] = 0.0;
					continue;
				}

				temp[i * (size + 1) + j] = matrix[k * (size + 1) + j] / matrix[k * (size + 1) + lead];
			}
			else
				temp[i * (size + 1) + j] = matrix[i * (size + 1) + j] - matrix[k * (size + 1) + j] / matrix[k * (size + 1) + lead] * matrix[i * (size + 1) + lead];
		}
	}

	copyMatrix(temp, matrix, size);
}

//ВЫЧИСЛЕНИЕ МАТРИЦЫ И ЛОГИЧЕСКОГО Q-ТЕРМА НА УЗЛЕ
double gauss_jordan_q(double *matrix, int size, int node, int k, bool *u){
	double *temp = allocationArrayTwo(size);
	double *prev = allocationArrayTwo(size);

	if(matrix[k * (size + 1) + node] != 0.0){
#pragma omp parallel num_thread(2)
		{
			*u = true;
			copyMatrix(matrix, prev, size);
			if(omp_get_thread_num() == 0){
				for(int i = 0; i < node; i++){
					if(prev[k * (size + 1) + i] != 0.0){
						if(k == 0 && node == 0)
							*u = *u && true;
						else
							*u = *u && false;
					}

					if(!(*u))
						break;
				}
			}
			else{
				calcMatrixToNode(matrix, temp, size, 0, node, u);
			}

			if(*u)
				PrintMatrix(matrix, size);
		}
	}

	closeArrayTwo(prev, size);
	closeArrayTwo(temp, size);

	return 0.0;
}

int main(int argc, char *argv[]){
	int rank, numnodes;
	double *test;
	int size;
	int k;
	bool *u = new bool;
	int end = 0;
	MPI_Status status;

	MPI_Init(&argc, &argv);

	MPI_Comm_size(MPI_COMM_WORLD, &numnodes);
	MPI_Comm_rank(MPI_COMM_WORLD, &rank);

	if(rank == 0){
		size = SIZE;
		test = allocationArrayTwo(size);
		InitSparseMatrix(test, size);

		MPI_Bcast(&size, 1, MPI_INT, rank, MPI_COMM_WORLD);

		for(int k = 0; k < size; k++){
			MPI_Bcast(&k, 1, MPI_INT, 0, MPI_COMM_WORLD);
			MPI_Bcast(test, size * (size + 1), MPI_DOUBLE, rank, MPI_COMM_WORLD);

			gauss_jordan_q(test, size, rank, k, u);

			if(!(*u))
				MPI_Recv(test, size * (size + 1), MPI_DOUBLE, MPI_ANY_SOURCE, MPI_ANY_TAG, MPI_COMM_WORLD, &status);
		}

		end = 1;
		MPI_Bcast(&end, 1, MPI_INT, 0, MPI_COMM_WORLD);

		closeArrayTwo(test, size);
	}
	else
	{
		MPI_Bcast(&size, 1, MPI_INT, 0, MPI_COMM_WORLD);

		do{
			test = allocationArrayTwo(size);
			MPI_Bcast(&k, 1, MPI_INT, 0, MPI_COMM_WORLD);
			MPI_Bcast(test, size * (size + 1), MPI_DOUBLE, 0, MPI_COMM_WORLD);

			gauss_jordan_q(test, size, rank, k, u);
			if(*u)
				MPI_Send(test, size * (size + 1), MPI_DOUBLE, 0, 123, MPI_COMM_WORLD);

			closeArrayTwo(test, size);

			MPI_Bcast(&end, 1, MPI_INT, 0, MPI_COMM_WORLD);
		}while(end != 1);

	}

	delete u;

	MPI_Finalize();

	return 0;
}