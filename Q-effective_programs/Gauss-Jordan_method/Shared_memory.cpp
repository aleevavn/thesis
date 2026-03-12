#include "omp.h"
#include <stdio.h>

double **allocationArrayTwo(int size);
void closeArrayTwo(double **arr, int size);
double ***allocationArrayThree(int size);
void closeArrayThree(double ***arr, int size);
void InitSparseMatrix(double **matrix, int size);
void PrintMatrix(double **matrix, int size);
int test();
void gauss_jordan_q(double **initial, int size, int nt);
void CopyMatrix(double **out, double **in, int size);

//ВЫДЕЛЕНИЕ ПАМЯТИ ДЛЯ ХРЕНЕНИЯ ДВУМЕРНОГО МАССИВА
double **allocationArrayTwo(int size){
	double **temp;

	temp = new double* [size];
	for(int i = 0; i < size; i++){
		temp[i] = new double [size + 1];
	}

	return temp;
}

//ОСВОБОЖДЕНИЕ ПАМЯТИ, ВЫДЕЛЕННОЙ ПОД ДВУМЕРНЫЙ МАССИВ
void closeArrayTwo(double **arr, int size){
	//если память не занята, выход из процедуры
	if(arr == NULL)
		return;

	for(int i = 0; i < size; i++){
		delete [] arr[i];
	}
	delete [] arr;
	arr = NULL;
}

//ВЫДЕЛЕНИЕ ПАМЯТИ ДЛЯ ХРАНЕНИЯ ТРЕХМЕРНОГО МАССИВА
double ***allocationArrayThree(int size){
	double ***temp;

	temp = new double** [size];
	for(int i = 0; i < size; i++){
		temp[i] = allocationArrayTwo(size);
	}

	return temp;
}

//ОСВОБОЖДЕНИЕ ПАМЯТИ, ВЫДЕЛЕННОЙ ПОД ТРЕХМЕРНЫЙ МАССИВ
void closeArrayThree(double ***arr, int size){
	//если память не занята, выход из процедуры
	if(arr == NULL)
		return;

	for(int i = 0; i < size; i++){
		closeArrayTwo(arr[i], size);
	}

	delete [] arr;
	arr = NULL;
}

//ИНИЦИАЛИЗАЦИЯ РАЗРЕЖЕННОЙ МАТРИЦЫ
void InitSparseMatrix(double **matrix, int size){
	double k = 1.0;

	for(int i = 0; i < size; i++){
		for(int j = 0; j < size + 1; j++){
			//главная диагональ
			if(i == j)
				matrix[i][j] = 1.0;
			else
				//столбец свободных членов
				if(j == size)
					matrix[i][j] = ++k;
				else
					//остальные элементы
					matrix[i][j] = 0.0;
		}
	}
}

void PrintMatrix(double **matrix, int size){
	for(int i = 0; i < size; i++){
		for(int j = 0; j < size + 1; j++){
			printf("%.2lf ", matrix[i][j]);
		}
		printf("\n");
	}
}

void CalcMatrixToStep(double **matrix, double **temp, int size, int k, int lead,bool *u){
	for(int i = 0; i < size; i++){
		//если логический Q-терм = false, завершение вычисления матрицы
		if(*u == false)
			break;

		for(int j = 0; j < size + 1; j++){
			//если логический Q-терм = false, завершение вычисление матрицы
			if(*u == false)
				break;

			if(k == i){
				//для избежания получения отрицательного нуля
				if(matrix[i][j] == 0.0){
					temp[i][j] = 0.0;
					continue;
				}

				temp[i][j] = matrix[k][j] / matrix[k][lead];
			}
			else{
				temp[i][j] = matrix[i][j] - matrix[k][j] / matrix[k][lead] * matrix[i][lead];
			}
		}
	}

	//из temp в matrix
	CopyMatrix(temp, matrix, size);
}

void CopyMatrix(double **out, double **in, int size){
	for(int i = 0; i < size; i++){
		for(int j = 0; j < size + 1; j++){
			in[i][j] = out[i][j];
		}
	}
}

void gauss_jordan_q(double **initial, int size, int nt){
	double **tek = NULL,
		**temp = NULL,
		**prev = NULL,
		**intermediate = NULL;
	bool *u;
	int n_threads;
	int n_zero = 0;

	intermediate = initial;
	prev = allocationArrayTwo(size);

	//количество ядер на одном узле "торнадо юургу" равно 24
	/*if(size < nt)
		n_threads = size;
	else
		n_threads = nt;*/
	n_threads = nt;

	for(int k = 0; k < size; k++){
		CopyMatrix(intermediate, prev, size);
#pragma omp parallel num_threads(n_threads)
		{
#pragma omp for schedule(static, 1) private(tek, temp, u)
			for(int lead = 0; lead < size; lead++){
				if(intermediate[k][lead] != 0.0){
					tek = allocationArrayTwo(size);
					temp = allocationArrayTwo(size);

					CopyMatrix(intermediate, tek, size);
					u = new bool;
					*u = true;

#pragma omp parallel num_threads(2)
					{
						if(omp_get_thread_num() == 0){
							for(int i = 0; i < lead; i++){
								if(prev[k][i] != 0.0){
									if(k == 0 && lead == 0)
										*u = *u && true;
									else
										*u = *u && false;
								}

								if(!(*u))
									break;
							}
						}
						else
						{
							//вычисление матрицы
							CalcMatrixToStep(tek, temp, size, k, lead, u);
						}
					}

					if(*u){
						CopyMatrix(tek, intermediate, size);
					}

					closeArrayTwo(tek, size);
					closeArrayTwo(temp, size);
					delete u;
				}//end if
			}//end цикл по lead
		}//end parallel
	}//end k

	//PrintMatrix(intermediate, size);
	closeArrayTwo(prev, size);

	/*FILE *f;
	f = fopen("1.txt", "w+");
	for(int i = 0; i < size; i++){
		for(int j = 0; j < size+1; j++){
			fprintf(f, "%.2lf ", intermediate[i][j]);
		}
		fprintf(f, "\n");
	}
	fclose(f);*/

}

//ТЕСТИРОВАНИЕ
int test(){
	int sizes[] = {500};
	int n_threads[] = {24};
	double **test = NULL;
	int sizes_len = sizeof(sizes) / sizeof(sizes[0]);
	double t1, t2;

	for(int t = 0; t < sizeof(n_threads) / sizeof(n_threads[0]); t++){
		for(int s = 0; s < sizes_len; s++){
			test = allocationArrayTwo(sizes[s]);
			InitSparseMatrix(test, sizes[s]);
			t1 = omp_get_wtime();
			gauss_jordan_q(test, sizes[s], n_threads[t]);
			t2 = omp_get_wtime();
			printf("%d:\n  %.20lf\n", sizes[s], t2-t1);
			closeArrayTwo(test, sizes[s]);
		}
	}
	return 0;
}

int main(){
	omp_set_nested(1);
	test();

	return 0;
}