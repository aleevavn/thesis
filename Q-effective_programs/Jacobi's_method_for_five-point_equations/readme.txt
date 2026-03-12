В файле simple_iteration.cpp код последовательной программы
В файле OpenMP.cpp код программы с использованием Open MP
В файле mpi.cpp код программы с использованием MPI

для запуска на суперкомпьютере

open mp:
компиляция icc -openmp ./simple.cpp -o simple
выполнение ./simple

MPI:
компиляция mpiicc -openmp ./mpi.cpp -o mpi , где mpi-имя файла.
поставить задачу в очередь sbatch -N 24 start24.sh , где -N 24 кол-во выделяемых узлов, start24.sh - имя запускаемого скрипта

