import Router from "express-promise-router";
import {authViaAuthToken, bodyParser} from "src";
import {log} from "util";
import malgorithmSchemas from "../../../../src/mongoDB/models/malgorithmSchemas";

const router = Router({
  mergeParams: true,
});
export default router;

/**
 * @api {get} /malgorithms/:malgorithmId Получить алгоритм
 * @apiVersion 2.0.0
 * @apiName GetMalgorithm
 * @apiGroup MAlgorithms
 * @apiPermission Гость
 *
 * @apiParam {string="1..9223372036854775807"} :algorithmId ID алгоритма
 *
 * @apiSuccess {object} data Данные
 * @apiSuccess {string="1..9223372036854775807"} data.id ID алгоритма
 * @apiSuccess {string} data.name Имя алгоритма
 * @apiSuccess {string} data.description Описание алгоритма
 * @apiSuccess {string} data.determinantsCount Количество Q-детерминантов алгоритма
 * @apiSuccess {object} data.data_height Результат аппроксимации функции высоты алгоритма
 * @apiSuccess {object} data.data_width Результат аппроксимации функции ширины алгоритма
 *
 * @apiError (Not Found 404 - Алгоритм не найден) {string="ALGORITHM_NOT_FOUND_BY_ID"} code Код ошибки
 * @apiError (Not Found 404 - Алгоритм не найден) {string} message Подробное описание ошибки
 *
 * @apiUse v200CommonErrors
 */
router.use(async (req, res) => {
  const malgorithm = await malgorithmSchemas.find({
    index: Number(req.params.malgorithmId),
  });

  if (req.query.type === "processors") {
    res.status(200).send({
      data: malgorithm[0].data_height,
    });
  }

  if (req.query.type === "ticks") {
    res.status(200).send({
      data: malgorithm[0].data_width,
    });
  }

  res.status(200).send(malgorithm);

  res.status(404).send();
});
