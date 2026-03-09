<template lang="html">
  <div>
    <input
      ref="uploadExpressionFileInput"
      style="display: none"
      type="file"
      @change="completeUploadExpression($event.target.files[0])"
    />
    <v-dialog v-model="newDeterminantData.dialog" max-width="50%" persistent>
      <v-card>
        <v-card-title>
          <span class="headline">{{
            $t("$.determinants.newDeterminant_form.header")
          }}</span>
        </v-card-title>
        <v-card-text>
          <v-form ref="newDeterminantForm">
            <v-text-field
              v-model="newDeterminantData.dimensions"
              :label="$t('$.determinants.newDeterminant_form.dimensions')"
              :readonly="newDeterminantData.loading"
              :rules="rules.dimensions"
            />
            <v-text-field
              v-model.number="newDeterminantData.iterations"
              :label="$t('$.determinants.newDeterminant_form.iterations')"
              min="0"
              :readonly="newDeterminantData.loading"
              :rules="rules.iterations"
              type="number"
            />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn
            :disabled="newDeterminantData.loading"
            flat
            @click="newDeterminantData.dialog = false"
          >
            {{ $t("$.determinants.newDeterminant_form.cancel") }}
          </v-btn>
          <v-btn
            color="primary"
            :loading="newDeterminantData.loading"
            @click="addDeterminant()"
          >
            {{ $t("$.determinants.newDeterminant_form.submit") }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-dialog v-model="editDeterminantData.dialog" max-width="50%" persistent>
      <v-card>
        <v-card-title>
          <span class="headline">{{
            $t("$.determinants.editDeterminant_form.header", {
              n: editDeterminantData.origin
                ? editDeterminantData.origin.id
                : "?",
            })
          }}</span>
        </v-card-title>
        <v-card-text>
          <v-form ref="editDeterminantForm">
            <v-text-field
              v-model="editDeterminantData.dimensions"
              :label="$t('$.determinants.editDeterminant_form.dimensions')"
              :readonly="editDeterminantData.loading"
              :rules="rules.dimensions"
            />
            <v-text-field
              v-model.number="editDeterminantData.iterations"
              :label="$t('$.determinants.editDeterminant_form.iterations')"
              min="0"
              :readonly="editDeterminantData.loading"
              :rules="rules.iterations"
              type="number"
            />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn
            :disabled="editDeterminantData.loading"
            flat
            @click="editDeterminantData.dialog = false"
          >
            {{ $t("$.determinants.editDeterminant_form.cancel") }}
          </v-btn>
          <v-btn
            color="primary"
            :loading="editDeterminantData.loading"
            @click="completeEditDeterminant()"
          >
            {{ $t("$.determinants.editDeterminant_form.submit") }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-breadcrumbs class="remove-top-padding" :items="breadcrumbItems">
      <template v-slot:divider>
        <v-icon>chevron_right</v-icon>
      </template>
    </v-breadcrumbs>
    <v-data-table
      class="elevation-1"
      :headers="determinantHeaders"
      :hide-actions="true"
      :items="determinantItems"
      :loading="uploadExpressionData.loading"
      :no-data-text="$t('$.determinants.noResultText')"
    >
      <template v-slot:headers="props">
        <tr>
          <th
            v-for="header in props.headers.filter((h) => h.text)"
            :key="header.text"
            class="text-xs-left"
          >
            {{ header.text }}
          </th>
          <th>
            <v-btn
              :disabled="!stateUser"
              flat
              small
              @click="newDeterminantData.dialog = true"
            >
              {{ $t("$.determinants.add") }}
            </v-btn>
          </th>
        </tr>
      </template>
      <template v-slot:items="props">
        <td>{{ props.item.id }}</td>
        <td>{{ props.item.dimensions.join(", ") }}</td>
        <td>{{ props.item.iterations }}</td>
        <td>{{ props.item.ticks === null ? "?" : props.item.ticks }}</td>
        <td>
          {{ props.item.processors === null ? "?" : props.item.processors }}
        </td>
        <td>
          <v-layout align-center fill-height justify-center row>
            <v-icon
              color="error"
              :disabled="
                !stateUser ||
                  (uploadExpressionData.loading &&
                    uploadExpressionData.origin === props.item)
              "
              small
              @click="deleteDeterminant(props.item)"
            >
              delete
            </v-icon>
            <v-icon
              class="ml-2"
              :disabled="
                !stateUser ||
                  (uploadExpressionData.loading &&
                    uploadExpressionData.origin === props.item)
              "
              small
              @click="beginEditDeterminant(props.item)"
            >
              edit
            </v-icon>
            <v-icon
              v-if="props.item.ticks === null || props.item.processors === null"
              class="ml-2"
              :disabled="!stateUser || uploadExpressionData.loading"
              small
              @click="beginUploadExpression(props.item)"
            >
              cloud_upload
            </v-icon>
            <a
              v-else
              class="ml-2"
              :href="generateExpressionDownloadLink(props.item)"
            >
              <v-icon small>cloud_download</v-icon>
            </a>
          </v-layout>
        </td>
      </template>
    </v-data-table>
  </div>
</template>

<script lang="ts">
import axios, {AxiosError} from "axios";
import _isEqual from "lodash.isequal";
import {Component, Mutation, State, Vue, Watch} from "nuxt-property-decorator";

@Component({
  async asyncData(context) {
    const [resAlgorithm, resDeterminants] = await Promise.all([
      axios.get(`/algorithms/${context.params.algorithmId}`, {
        baseURL: process.env.API_ENDPOINT,
      }),
      axios.get(`/algorithms/${context.params.algorithmId}/determinants`, {
        baseURL: process.env.API_ENDPOINT,
      }),
    ]);

    return {
      algorithmData: resAlgorithm.data.data,
      determinantItems: resDeterminants.data.data,
    };
  },
})
export default class DeterminantsPage extends Vue {
  public algorithmData!: any;

  public determinantItems!: any[];

  public newDeterminantData = {
    dialog: false,
    dimensions: "",
    iterations: "" as any,
    loading: false,
  };

  public editDeterminantData = {
    dialog: false,
    dimensions: "",
    iterations: "" as any,
    loading: false,
    origin: null as any,
  };

  public uploadExpressionData = {
    loading: false,
    origin: null as any,
  };

  @State("user")
  public stateUser!: string;

  @Mutation("SET_ERROR")
  public setStateError!: (payload: {
    errorMessage?: string;
    showError?: boolean;
  }) => void;

  public get determinantHeaders() {
    return [
      {
        text: "ID",
      },
      {
        text: this.$t("$.determinants.headers.dimensions"),
      },
      {
        text: this.$t("$.determinants.headers.iterations"),
      },
      {
        text: this.$t("$.determinants.headers.ticks"),
      },
      {
        text: this.$t("$.determinants.headers.processors"),
      },
      {},
    ];
  }

  public get breadcrumbItems() {
    return [
      {
        exact: true,
        nuxt: true,
        text: this.$t("$.determinants.breadcrumbs.prev"),
        to: "/",
      },
      {
        exact: true,
        nuxt: true,
        text: this.$t("$.determinants.breadcrumbs.cur", {
          n: this.algorithmData.id,
          name: this.algorithmData.name,
        }),
        to: `/algorithms/${this.algorithmData.id}/determinants`,
      },
    ];
  }

  public get rules() {
    return {
      dimensions: [
        (v: string) => {
          const digits = (v || "")
            .split(/[ ,]/)
            .filter((x) => !!x)
            .map((x) => parseInt(x, 10));
          if (digits.length === 0) {
            return this.$t("$.rules.dimensions.required");
          }
          return true;
        },
      ],
      iterations: [
        (v: string) => v !== "" || this.$t("$.rules.iterations.required"),
        (v: number) => v >= 0 || this.$t("$.rules.iterations.>=0"),
        (v: number) =>
          v === parseInt(String(v), 10) || this.$t("$.rules.iterations.int"),
      ],
    };
  }

  @Watch("newDeterminantData.dialog")
  public onNewDeterminantDialogChanged(): void {
    if (!this.newDeterminantData.dialog) {
      (this.$refs.newDeterminantForm as any).reset();
    }
  }

  @Watch("editDeterminantData.dialog")
  public onEditDeterminantDialogChanged(): void {
    if (!this.editDeterminantData.dialog) {
      (this.$refs.editDeterminantForm as any).reset();
    }
  }

  public generateExpressionDownloadLink({algorithmId, id}: any): string {
    return `${
      process.env.API_ENDPOINT
    }/algorithms/${algorithmId}/determinants/${id}/expression/${algorithmId}-${id}.json`;
  }

  public async addDeterminant(): Promise<void> {
    if (!(this.$refs.newDeterminantForm as any).validate()) {
      return;
    }
    this.newDeterminantData.loading = true;
    try {
      const resId = await axios.post(
        `/algorithms/${this.algorithmData.id}/determinants`,
        {
          dimensions: this.newDeterminantData.dimensions
            .split(/[ ,]/)
            .filter((x) => !!x)
            .map((x) => parseInt(x, 10)),
          iterations: this.newDeterminantData.iterations,
        },
        {
          baseURL: process.env.API_ENDPOINT,
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
        },
      );

      const resDeterminant = await axios.get(
        `/algorithms/${this.algorithmData.id}/determinants/${resId.data.id}`,
        {
          baseURL: process.env.API_ENDPOINT,
        },
      );

      this.determinantItems.push(resDeterminant.data.data);
      this.determinantItems.sort((x, y) => {
        const xId = parseInt(x.id, 10);
        const yId = parseInt(y.id, 10);
        return xId < yId ? -1 : xId > yId ? 1 : 0;
      });

      this.newDeterminantData.dialog = false;
    } catch (error) {
      const axiosError = error as AxiosError;
      if (axiosError.response) {
        if (
          axiosError.response.status === 409 &&
          axiosError.response.data.code ===
            "DETERMINANT_FOUND_BY_DIMENSIONS_OR_ITERATIONS"
        ) {
          this.setStateError({
            errorMessage: this.$t(
              "$.determinants.errors.DETERMINANT_FOUND_BY_DIMENSIONS_OR_ITERATIONS",
            ) as string,
            showError: true,
          });
          return;
        }
      }
      throw error;
    } finally {
      this.newDeterminantData.loading = false;
    }
  }

  public async deleteDeterminant(determinant: any): Promise<void> {
    if (confirm(this.$t("$.determinants.deleteConfirm") as string)) {
      const index = this.determinantItems.indexOf(determinant);
      this.determinantItems.splice(index, 1);

      await axios.delete(
        `/algorithms/${determinant.algorithmId}/determinants/${determinant.id}`,
        {
          baseURL: process.env.API_ENDPOINT,
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
        },
      );
    }
  }

  public beginUploadExpression(determinant: any): void {
    this.uploadExpressionData.origin = determinant;
    (this.$refs.uploadExpressionFileInput as any).click();
  }

  public async completeUploadExpression(file: File): Promise<void> {
    this.uploadExpressionData.loading = true;
    try {
      await axios.put(
        `/algorithms/${
          this.uploadExpressionData.origin.algorithmId
        }/determinants/${this.uploadExpressionData.origin.id}/expression`,
        file,
        {
          baseURL: process.env.API_ENDPOINT,
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
            "Content-Type": "application/json",
          },
        },
      );

      const res = await axios.get(
        `/algorithms/${
          this.uploadExpressionData.origin.algorithmId
        }/determinants/${this.uploadExpressionData.origin.id}`,
        {
          baseURL: process.env.API_ENDPOINT,
        },
      );
      this.uploadExpressionData.origin.processors = res.data.data.processors;
      this.uploadExpressionData.origin.ticks = res.data.data.ticks;
    } catch (error) {
      const axiosError = error as AxiosError;
      if (axiosError.response) {
        if (
          axiosError.response.status === 400 &&
          axiosError.response.data.code === "BAD_REQUEST"
        ) {
          this.setStateError({
            errorMessage: this.$t(
              "$.determinants.errors.BAD_REQUEST",
            ) as string,
            showError: true,
          });
          return;
        }
        if (axiosError.response.status === 413) {
          this.setStateError({
            errorMessage: this.$t(
              "$.determinants.errors.PAYLOAD_TOO_LARGE",
            ) as string,
            showError: true,
          });
          return;
        }
      }
      throw error;
    } finally {
      (this.$refs.uploadExpressionFileInput as any).value = "";
      this.uploadExpressionData.loading = false;
      this.uploadExpressionData.origin = null;
    }
  }

  public beginEditDeterminant(determinant: any): void {
    this.editDeterminantData.origin = determinant;
    this.editDeterminantData.dimensions = determinant.dimensions.join(", ");
    this.editDeterminantData.iterations = determinant.iterations;
    this.editDeterminantData.dialog = true;
  }

  public async completeEditDeterminant(): Promise<void> {
    if (!(this.$refs.editDeterminantForm as any).validate()) {
      return;
    }

    const dimensions = this.editDeterminantData.dimensions
      .split(/[ ,]/)
      .filter((x) => !!x)
      .map((x) => parseInt(x, 10));

    const data = {
      dimensions: _isEqual(
        this.editDeterminantData.origin.dimensions,
        dimensions,
      )
        ? undefined
        : dimensions,
      iterations:
        this.editDeterminantData.origin.iterations ===
        this.editDeterminantData.iterations
          ? undefined
          : this.editDeterminantData.iterations,
    };

    if (Object.values(data).every((v) => v === undefined)) {
      this.editDeterminantData.dialog = false;
      return;
    }

    this.editDeterminantData.loading = true;
    try {
      await axios.patch(
        `/algorithms/${
          this.editDeterminantData.origin.algorithmId
        }/determinants/${this.editDeterminantData.origin.id}`,
        data,
        {
          baseURL: process.env.API_ENDPOINT,
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
        },
      );

      this.editDeterminantData.origin.dimensions = dimensions;
      this.editDeterminantData.origin.iterations = this.editDeterminantData.iterations;
      this.editDeterminantData.dialog = false;
    } catch (error) {
      const axiosError = error as AxiosError;
      if (axiosError.response) {
        if (
          axiosError.response.status === 409 &&
          axiosError.response.data.code ===
            "DETERMINANT_FOUND_BY_DIMENSIONS_OR_ITERATIONS"
        ) {
          this.setStateError({
            errorMessage: this.$t(
              "$.determinants.errors.DETERMINANT_FOUND_BY_DIMENSIONS_OR_ITERATIONS",
            ) as string,
            showError: true,
          });
          return;
        }
      }
      throw error;
    } finally {
      this.editDeterminantData.loading = false;
    }
  }
}
</script>

<style lang="css" scoped>
.remove-top-padding {
  padding-top: 0 !important;
}
</style>
