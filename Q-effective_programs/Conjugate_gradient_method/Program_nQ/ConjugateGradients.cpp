#include <iostream>
#include <fstream>
#include <cstdlib>
using namespace std;

double dabs(double x) {
	return x>=0?x:-x;
}

bool isSignificant(double x,double threshold) {
	return dabs(x)>=threshold?true:false;
}

void conjgrads(int n,double **a,double *b,double *x0,double *x,double maxaccerr) {
	int count=0;
// Объявление нужных векторов и чисел
// Текущее приближение
	double *ongoing;
	ongoing=new double[n];
	#pragma omp parallel for private(i) {
	for (int i=0;i<n;i++) ongoing[i]=x0[i];
	#pragma omp parallel }
// Невязка
	double *r;
	r=new double[n];
	#pragma omp parallel for private(i) {
	for (int i=0;i<n;i++) r[i]=b[i];
	for (int i=0;i<n;i++) for (int j=0;j<n;j++) r[i]-=a[i][j]*x0[j];
	#pragma omp parallel }
// Направление
	double *direction;
	direction=new double[n];
	#pragma omp parallel for private(i) shared(n,r,direction) {
	for (int i=0;i<n;i++) direction[i]=r[i];
	#pragma omp parallel }
	double error=100;
// Вспомогательные переменные
	double A,a1,a2,B,b1,b2;
	double *a3;
	a3=new double[n];
	double *oldr;
	oldr=new double[n];
	double *oldp;
	oldp=new double[n];
	double *previous;
	previous=new double[n];
// Поиск решения
	while (error>=maxaccerr) {
		count++;
// Вычисление коэффициента A
		#pragma omp parallel for private(i) {
		for (int i=0;i<n;i++) a3[i]=0;
		for (int i=0;i<n;i++) oldr[i]=r[i];
		for (int i=0;i<n;i++) oldp[i]=direction[i];
		for (int i=0;i<n;i++) previous[i]=ongoing[i];
		#pragma omp parallel }
		a1=0;
		a2=0;
		b1=0;
		b2=0;
		#pragma omp parallel for private(i,j) {
		for (int i=0;i<n;i++) a1+=r[i]*r[i];
		for (int i=0;i<n;i++) for (int j=0;j<n;j++) a3[i]+=direction[j]*a[j][i];
		#pragma omp parallel }
		#pragma openmp parallel for private(i) {
		for (int i=0;i<n;i++) a2+=a3[i]*direction[i];
		#pragma omp parallel }
		A=a1/a2;
// Вычисление нового приближения
		#pragma omp parallel for private(i,j) shared(n,ongoing,A,direction,a) {
		for (int i=0;i<n;i++) ongoing[i]+=A*direction[i];
		#pragma omp parallel }
// Вычисление новой невязки
		for (int i=0;i<n;i++) for (int j=0;j<n;j++) r[i]-=A*a[i][j]*direction[j];
// Вычисление значения B
		for (int i=0;i<n;i++) b1+=r[i]*r[i];
		for (int i=0;i<n;i++) b2+=oldr[i]*oldr[i];
		B=b1/b2;
// Вычисление нового направления
		for (int i=0;i<n;i++) direction[i]=r[i]+B*direction[i];
// Вычисление погрешности
		error=dabs(previous[0]-ongoing[0]);
		#pragma omp parallel for private(i) {
		for (int i=1;i<n;i++) error=dabs(previous[i]-ongoing[i])>error?dabs(previous[i]-ongoing[i]):error;
		#pragma omp parallel }
		double R=0;
		#pragma omp parallel for private(i) shared(R,r) {
		for (int i=0;i<n;i++) R+=r[i]*r[i];
		#pragma omp parallel }
		double D=0;
		#pragma omp parallel for private(i) shared(D,direction) {
		for (int i=0;i<n;i++) D+=direction[i]*direction[i];
		#pragma omp parallel }
	}
// Освобождение памяти и возвращение результата
	for (int i=0;i<n;i++) x[i]=ongoing[i];
	delete [] ongoing;
	delete [] r;
	delete [] direction;
	delete [] a3;
	delete [] oldr;
	delete [] oldp;
	delete [] previous;
	return;
}

int main(int argc,char *argv[]) {
	if (argc==1) {
		printf("No file given.\n");
		return 2;
	}
	int n;
	char readthis[100];
// Чтение файла
	ifstream source(argv[1]);
	source>>readthis;
	n=atoi(readthis);
	double **leftpart;
	leftpart=new double*[n];
	for (int i=0;i<n;i++) leftpart[i]=new double[n];
	double *rightpart;
	rightpart=new double[n];
	double *start;
	start=new double[n];
	double *solution;
	solution=new double[n];
	for (int i=0;i<n;i++) {
		for (int j=0;j<n;j++) {
			source>>readthis;
			leftpart[i][j]=atof(readthis);
		}			
		source>>readthis;
		rightpart[i]=atof(readthis);
	}
	for (int i=0;i<n;i++) {
		source>>readthis;
		start[i]=atof(readthis);
	}
	source>>readthis;
	double err=atof(readthis);
	source.close();
// Решение уравнения
	conjgrads(n,leftpart,rightpart,start,solution,err);
	delete [] leftpart;
	for (int i=0;i<n;i++) cout <<solution[i]<<"\n";
	delete [] rightpart;
	delete [] solution;
	delete [] start;
	return 0;
}
