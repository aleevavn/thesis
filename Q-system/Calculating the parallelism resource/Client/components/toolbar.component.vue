<template lang="html">
  <v-toolbar style="background: #A9A9A9;" app dense>
    <v-dialog v-model="signinData.dialog" max-width="50%" persistent>
      <v-card>
        <v-card-title>
          <span class="headline">{{ $t("$.toolbar.signin_form.header") }}</span>
        </v-card-title>
        <v-card-text>
          <v-form ref="signinForm">
            <v-text-field
              v-model="signinData.user"
              :label="$t('$.toolbar.signin_form.name')"
              prepend-icon="person"
              :readonly="signinData.loading"
              :rules="rules.user"
            />
            <v-text-field
              v-model="signinData.password"
              :label="$t('$.toolbar.signin_form.password')"
              prepend-icon="lock"
              :readonly="signinData.loading"
              :rules="rules.password"
              type="password"
            />
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn
            :disabled="signinData.loading"
            flat
            @click="signinData.dialog = false"
          >
            {{ $t("$.toolbar.signin_form.cancel") }}
          </v-btn>
          <v-btn
            color="primary"
            :loading="signinData.loading"
            @click="signin()"
          >
            {{ $t("$.toolbar.signin_form.submit") }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-toolbar-title>
      <nuxt-link class="toolbar-title-color" to="/">
        Q-system
      </nuxt-link>
    </v-toolbar-title>
    <v-spacer />
    <v-toolbar-items>
      <v-btn flat @click="changeLocale()">
        <v-icon>language</v-icon>&nbsp;{{ $i18n.locale }}
      </v-btn>
      <v-btn flat @click="$router.push('/about')">
        <v-icon>question_answer</v-icon>&nbsp;{{ $t("$.toolbar.about") }}
      </v-btn>
      <v-btn v-if="!stateUser" flat @click="signinData.dialog = true">
        <v-icon>meeting_room</v-icon>&nbsp;{{ $t("$.toolbar.signin") }}
      </v-btn>
      <v-btn v-else flat @click="signout()">
        <v-icon>no_meeting_room</v-icon>&nbsp;{{ stateUser }}
      </v-btn>
    </v-toolbar-items>
  </v-toolbar>
</template>

<script lang="ts">
import axios, {AxiosError} from "axios";
import {Component, Mutation, State, Vue, Watch} from "nuxt-property-decorator";

@Component
export default class ToolbarComponent extends Vue {
  public signinData = {
    dialog: false,
    loading: false,
    password: "",
    user: "",
  };

  public get rules() {
    return {
      password: [(v: string) => !!v || this.$t("$.rules.password.required")],
      user: [(v: string) => !!v || this.$t("$.rules.user.required")],
    };
  }

  @State("user")
  public stateUser!: string;

  @Mutation("SET_ERROR")
  public setStateError!: (payload: {
    errorMessage?: string;
    showError?: boolean;
  }) => void;

  @Mutation("SET_USER")
  public setStateUser!: (payload: {user: string | null}) => void;

  @Watch("signinData.dialog")
  public onSigninDataDialogChanged(): void {
    if (!this.signinData.dialog) {
      (this.$refs.signinForm as any).reset();
    }
  }

  public changeLocale(): void {
    this.$i18n.locale = this.$i18n.locale === "en" ? "ru" : "en";
    localStorage.setItem("locale", this.$i18n.locale);
  }

  public async signin(): Promise<void> {
    if (!(this.$refs.signinForm as any).validate()) {
      return;
    }
    this.signinData.loading = true;
    try {
      const resWithToken = await axios.post(
        "/signin",
        {
          password: this.signinData.password,
          user: this.signinData.user,
        },
        {
          baseURL: process.env.API_ENDPOINT,
        },
      );
      localStorage.setItem("token", resWithToken.data.token);

      const resWithUser = await axios.get("/user", {
        baseURL: process.env.API_ENDPOINT,
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
        },
      });
      this.setStateUser({user: resWithUser.data.data.user});

      this.signinData.dialog = false;
    } catch (error) {
      const axiosError: AxiosError = error;
      if (axiosError.response) {
        if (
          axiosError.response.status === 401 &&
          axiosError.response.data.code === "WRONG_USER_OR_PASSWORD"
        ) {
          this.setStateError({
            errorMessage: this.$t(
              "$.toolbar.signin_form.errors.WRONG_USER_OR_PASSWORD",
            ) as string,
            showError: true,
          });
          return;
        }
      }
      throw error;
    } finally {
      this.signinData.loading = false;
    }
  }

  public signout(): void {
    this.setStateUser({user: null});
    localStorage.removeItem("token");
  }
}
</script>

<style lang="css" scoped>
.toolbar-title-color {
  color: inherit;
}
</style>
