import Router from "express-promise-router";
import * as assert from "http-assert";

import {Algorithm, ApiCodes} from "src";

const router = Router({
  mergeParams: true,
});
export default router;

/**
 * @api {get} /algorithms/:algorithmId Получить алгоритм
 * @apiVersion 2.0.0
 * @apiName GetAlgorithm
 * @apiGroup Algorithms
 * @apiPermission Гость
 *
 * @apiParam {string="1..9223372036854775807"} :algorithmId ID алгоритма
 *
 * @apiSuccess {object} data Данные
 * @apiSuccess {string="1..9223372036854775807"} data.id ID алгоритма
 * @apiSuccess {string} data.name Имя алгоритма
 * @apiSuccess {string} data.description Описание алгоритма
 * @apiSuccess {string} data.determinantsCount Количество Q-детерминантов алгоритма
 *
 * @apiError (Not Found 404 - Алгоритм не найден) {string="ALGORITHM_NOT_FOUND_BY_ID"} code Код ошибки
 * @apiError (Not Found 404 - Алгоритм не найден) {string} message Подробное описание ошибки
 *
 * @apiUse v200CommonErrors
 */
router.use(async (req, res) => {
  const algorithm = (await Algorithm.findByPk(
    req.params.algorithmId,
  )) as Algorithm;
  assert(algorithm, 404, "algorithm with same id not found", {
    code: ApiCodes.ALGORITHM_NOT_FOUND_BY_ID,
  });

  res.status(200).json({
    data: {
      description: algorithm.description,
      determinantsCount: algorithm.determinantsCount,
      id: algorithm.id,
      name: algorithm.name,
    },
  });
});
