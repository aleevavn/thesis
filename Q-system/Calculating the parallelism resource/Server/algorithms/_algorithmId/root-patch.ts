import {celebrate} from "celebrate";
import Router from "express-promise-router";

import {Algorithm, authViaAuthToken, bodyParser, Joi} from "src";

const router = Router({
  mergeParams: true,
});
export default router;

/**
 * @api {patch} /algorithms/:algorithmId Изменить алгоритм
 * @apiVersion 2.0.0
 * @apiName ChangeAlgorithm
 * @apiGroup Algorithms
 * @apiPermission Пользователь
 *
 * @apiParam {string="1..9223372036854775807"} :algorithmId ID алгоритма
 * @apiParam {string} [name] Имя алгоритма
 * @apiParam {string} [description] Описание алгоритма
 *
 * @apiUse v200CommonErrors
 * @apiUse v200AuthViaAuthToken
 */
router.use(
  authViaAuthToken,
  bodyParser(),
  celebrate({
    body: Joi.object({
      description: Joi.string(),
      name: Joi.string(),
    }),
  }),
  async (req, res) => {
    await Algorithm.update(
      {
        description: req.body.description,
        name: req.body.name,
      },
      {
        returning: false,
        where: {
          id: req.params.algorithmId,
        },
      },
    );

    res.status(204).send();
  },
);
